# StateChartsDotNet

[![Gitter](https://badges.gitter.im/StateChartsDotNet/community.svg)](https://gitter.im/StateChartsDotNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## What are statecharts?

The best definition comes from [here](https://statecharts.github.io/what-is-a-statechart.html):

> The primary feature of statecharts is that states can be organized in a hierarchy:  A statechart is a [state machine](https://statecharts.github.io/what-is-a-state-machine.html) where each state in the state machine may define its own subordinate state machines, called substates.  Those states can again define substates.

The utility of traditional state machines goes down as system complexity increases, due to [state explosion](https://statecharts.github.io/state-machine-state-explosion.html). Also, state machines by themselves are merely a description of behavior, not behavior itself. Statecharts (and StateChartsDotNet) address both of these limitations.

## Goals

StateChartsDotNet aims to provide a full-featured statechart implementation for the .NET Core runtime, enabling SCDN to run nicely on Windows, Mac, Linux, and all your favorite clouds. Maybe even your [browser](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)?

Some specific design and implementation choices:

- An [abstraction](./Common/Model) for describing statecharts that allows for [multiple](./Metadata.Xml) [implementations](./Metadata.Fluent) _(perhaps you've got another one in mind?)_

- Two behaviorally equivalent statechart execution engines:

  - A fast, in-memory [implementation](./Engine)

  - A durable, reliable [implementation](./DurableEngine) based on the [Durable Task framework](https://github.com/Azure/durabletask)

- Abstractions for both [pull](./Common/Model/Execution/IQueryMetadata.cs)- and [push](./Common/Model/Execution/ISendMessageMetadata.cs)-based communication with external systems; talk to all your favorite native cloud services from within your statechart!

- In addition to parent-child state relationships _within_ a single statechart, there is also support for parent-child relationships _between_ statechart instances (execute statechart A within the context of statechart B, etc.)

## Usage

### Fluent API

```csharp
var x = 1;

var machine = StateChart.Define("test")
                        .AtomicState("state1")
                            .OnEntry()
                                .Execute(_ => x += 1)
                                .Attach()
                            .OnExit()
                                .Execute(_ => x += 1)
                                .Attach()
                            .Transition()
                                .Target("alldone")
                                .Attach()
                            .Attach()
                        .FinalState("alldone")
                            .Attach();

var context = new ExecutionContext(machine);

await context.StartAndWaitForCompletionAsync();
```

### [SCXML-compliant](https://www.w3.org/TR/scxml/) API

```csharp
var xmldoc = @"<?xml version='1.0'?>
                <scxml xmlns='http://www.w3.org/2005/07/scxml'
                        version='1.0'
                        datamodel='csharp'>
                    <state id='state1'>
                        <onentry>
                            <http-post>
                                <url>http://localhost:4444/</url>
                                <body>
                                    { value: 5 }
                                </body>
                            </http-post>
                        </onentry>
                        <transition target='alldone' />
                    </state>
                    <final id='alldone' />
                </scxml>";

var machine = new StateChart(XDocument.Parse(xmldoc));

var context = new ExecutionContext(machine, _logger);

await context.StartAndWaitForCompletionAsync();
```

### JSON API

```csharp
var json = @"{
                 'states': [
                     {
                         'id': 'state1',
                         'onentry': {
                             'content': [
                                 {
                                     'type': 'http-post',
                                     'url': 'http://localhost:4444/',
                                     'body': {
                                         'value': 5
                                     }
                                 }
                             ]
                         },
                         'transitions': [
                             { 'target': 'alldone' }
                         ]
                     },
                     {
                         'id': 'alldone',
                         'type': 'final'
                     }
                 ]
             }";

var machine = new StateChart(JObject.Parse(json));

var context = new ExecutionContext(machine, _logger);

await context.StartAndWaitForCompletionAsync();
```

### Durable (fault-tolerant) execution

```csharp
var machine = GetStateChart();

var emulator = new LocalOrchestrationService();             // any durable task orchestration service implementation

var storage = new Durable.InMemoryOrchestrationStorage();   // metadata storage for in-flight statechart instances

var executionTimeout = TimeSpan.FromMinutes(1);

var context = new Durable.ExecutionContext(machine, emulator, storage, cancelToken, executionTimeout);

await context.StartAndWaitForCompletionAsync();
```

### Parent-child statecharts

```csharp
var x = 1;

var innerMachine = StateChart.Define("inner")
                    .AtomicState("innerState1")
                        .OnEntry()
                            .Execute(_ => x += 1)
                            .Attach()
                        .OnExit()
                            .Execute(_ => x += 1)
                            .Attach()
                        .Transition()
                            .Target("alldone")
                            .Attach()
                        .Attach()
                    .FinalState("alldone")
                        .Attach();

var machine = StateChart.Define("outer")
                        .AtomicState("outerState1")
                            .InvokeStateChart()
                                .Definition(innerMachine)
                                .Attach()
                            .Transition()
                                .Message("done.invoke.*")
                                .Target("alldone")
                                .Attach()
                            .Attach()
                        .FinalState("alldone")
                            .Attach();

var context = new ExecutionContext(machine);

await context.StartAndWaitForCompletionAsync();
```

### [ASP.NET Core host](./Web)

#### /api/registerandstart request example payload
```json
{
    "inputs": {
        "x": 5
    },
    "statechart": {
        "name": "outer",
        "states": [
            {
                "id": "outerState1",
                "invokes": [
                    {
                        "params": [
                            { "name": "x", "location": "x" }
                        ],
                        "content": {
                            "name": "inner",
                            "states": [
                                {
                                    "id": "innerState1",
                                    "onentry": {
                                        "content": [
                                            { "type": "assign", "location": "x", "expr": "x * 2" }
                                        ]
                                    },
                                    "transitions": [
                                        { "target": "alldone" }
                                    ]
                                },
                                {
                                    "id": "alldone",
                                    "type": "final",
                                    "donedata": {
                                        "params": [
                                            { "name": "innerX", "location": "x" }
                                        ]
                                    }
                                }
                            ]
                        },
                        "finalize": [
                            { "type": "assign", "location": "innerX", "expr": "_event.Parameters[\"innerX\"]" }
                        ]
                    }
                ],
                "transitions": [
                    { "event": "done.invoke.*", "target": "alldone" }
                ]
            },
            { "id": "alldone", "type": "final" }
        ]
    }
}
```

#### /api/status response example payload
```json
{
    "startTime": "2020-12-18T18:15:12.3280769Z",
    "endTime": "2020-12-18T18:15:24.4911782Z",
    "lastUpdateTime": "2020-12-18T18:15:24.4914133Z",
    "status": "Completed",
    "instanceId": "030c677c251a463485dbb409f146fecf",
    "input": {
        "x": 5
    },
    "output": {
        "x": 5,
        "innerX": 10
    },
    "error": null
}
```

## Background and Resources

- Excellent, pragmatic statechart explainer [here](https://statecharts.github.io/)

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts
