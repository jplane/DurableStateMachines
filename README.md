# StateChartsDotNet

[![Gitter](https://badges.gitter.im/StateChartsDotNet/community.svg)](https://gitter.im/StateChartsDotNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## What are statecharts?

The best definition comes from [here](https://statecharts.github.io/what-is-a-statechart.html):

> The primary feature of statecharts is that states can be organized in a hierarchy:  A statechart is a [state machine](https://statecharts.github.io/what-is-a-state-machine.html) where each state in the state machine may define its own subordinate state machines, called substates.  Those states can again define substates.

The utility of traditional state machines goes down as system complexity increases, due to [state explosion](https://statecharts.github.io/state-machine-state-explosion.html). Also, state machines by themselves are merely a description of behavior, not behavior itself. Statecharts (and StateChartsDotNet) address both of these limitations.

## Goals

StateChartsDotNet aims to provide a full-featured statechart implementation for the .NET Core runtime, enabling SCDN to run nicely on Windows, Mac, Linux, and all your favorite clouds. Included in the repo are both an ASP.NET Core Web API [host](./WebHost) and an Azure Functions [host](./FunctionHost) implemented as a [custom handler](https://docs.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers).

Some specific design and implementation choices:

- An [abstraction](./Common/Model) for describing statecharts that allows for [multiple](./Metadata.Xml) [implementations](./Metadata.Fluent) _(perhaps you've got another one in mind?)_

- Run in-memory for fastest execution, or opt into durable storage of engine execution state for resilience in the face of failures

- Abstractions for both [pull](./Common/Model/Execution/IQueryMetadata.cs)- and [push](./Common/Model/Execution/ISendMessageMetadata.cs)-based communication with external systems; talk to all your favorite native cloud services from within your statechart!

- In addition to parent-child state relationships _within_ a single statechart, there is also support for parent-child relationships _between_ statechart instances (execute statechart A within the context of statechart B, etc.)

## Getting Started

### Where do you want to run statecharts?

1. Hosted in a generic REST API - use the pre-built ASP.NET Core [WebHost](./WebHost) and start with the [example HTTP calls](./WebHost/statecharts.postman_collection.json) for registering and running statecharts

2. Hosted in an Azure Functions app - use the pre-built [FunctionHost](./FunctionHost) and start with the [example HTTP calls](./WebHost/statecharts.postman_collection.json) for registering and running statecharts

3. In your own app - the core statechart engines are built as [netstandard2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) libs which means you can use them virtually anywhere .NET Core runs

### Optimize for performance, or reliability?

SCDN supports two execution engines:

  - A fast, in-memory [implementation](./Engine)

  - A durable, reliable [implementation](./DurableEngine) based on the [Durable Task framework](https://github.com/Azure/durabletask)

The results from executing your statechart will be identical with either engine; you simply choose performance vs. reliability to suit your scenario.

### Define your statechart

Statecharts support standard state machine concepts like [atomic](https://statecharts.github.io/glossary/atomic-state.html) and [parallel](https://statecharts.github.io/glossary/parallel-state.html) states, state [transitions](https://statecharts.github.io/glossary/transition.html), [actions](https://statecharts.github.io/glossary/action.html) within states, etc.

Statecharts also support advanced concepts like [history states](https://statecharts.github.io/glossary/history-state.html), [event-based transitions](https://statecharts.github.io/glossary/event.html), and [nested or compound state hierarchies](https://statecharts.github.io/glossary/compound-state.html). 

In SCDN you define statecharts using declarative metadata in JSON, XML, or C# fluent syntax. See below for examples of each.

For advanced scenarios, you can also define your own metadata syntax and map it to primitives the SCDN engines can interpret and execute directly.

## Examples

### Fluent API

```csharp
static void action(dynamic data) => data.x += 1;

var machine = StateChart.Define("test")
                        .DataInit("x", 1)
                        .State("state1")
                            .OnEntry
                                .Execute(action)._
                            .OnExit
                                .Execute(action)._
                            .Transition
                                .Target("alldone")._._
                        .FinalState("alldone")._;

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
static object getValue(dynamic data) => data.x + 1;

var innerMachine = StateChart.Define("inner")
                             .DataInit("x", 1)
                             .State("innerState1")
                                 .OnEntry
                                     .Assign("x", getValue)._
                                 .OnExit
                                     .Assign("x", getValue)._
                                 .Transition
                                     .Target("alldone")._._
                             .FinalState("alldone")._;

var machine = StateChart.Define("outer")
                        .State("outerState1")
                            .InvokeStateChart
                                .Definition(innerMachine)._
                            .Transition
                                .Message("done.invoke.*")
                                .Target("alldone")._._
                        .FinalState("alldone")._;

var context = new ExecutionContext(machine);

await context.StartAndWaitForCompletionAsync();
```

### [ASP.NET Core Web API host](./WebHost) and [Azure Functions host](./FunctionHost)

Add a new statechart definition for later execution:
>POST /api/register

Lookup a registered statechart definition:
>GET /api/metadata/{metadataId}

Create a new statechart instance:
>POST /api/start/{metadataId}

Register definition and start an instance in a single request:
>POST /api/registerandstart

Stop an existing statechart instance:
>PUT /api/stop/{instanceId}

Send a message to an existing statechart instance:
>PUT /api/sendmessage/{instanceId}

Get the status of an existing statechart instance:
>GET /api/{instanceId}

(Postman examples [here](./WebHost/statecharts.postman_collection.json))

## Background and Resources

- Excellent, pragmatic statechart explainer [here](https://statecharts.github.io/)

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts
