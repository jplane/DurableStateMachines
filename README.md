# StateChartsDotNet

[![Gitter](https://badges.gitter.im/StateChartsDotNet/community.svg)](https://gitter.im/StateChartsDotNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## What are statecharts?

The best definition comes from [here](https://statecharts.github.io/what-is-a-statechart.html):

> The primary feature of statecharts is that states can be organized in a hierarchy:  A statechart is a [state machine](https://statecharts.github.io/what-is-a-state-machine.html) where each state in the state machine may define its own subordinate state machines, called substates.  Those states can again define substates.

The utility of state machines goes down as system complexity increases, due to [state explosion](https://statecharts.github.io/state-machine-state-explosion.html). Also, state machines by themselves are merely a description of behavior, not behavior itself. Statecharts (and StateChartsDotNet) address both of these limitations.

## Goals

StateChartsDotNet aims to provide a full-featured statechart implementation for the .NET Core runtime, enabling SCDN to run nicely on Windows, Mac, Linux, and all your favorite clouds. Maybe even your [browser](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)?

Some specific design and implementation choices:

- An [abstraction](./Common/Model) for describing statecharts that allows for [multiple](./Metadata.Xml) [implementations](./Metadata.Fluent) _(perhaps you've got another one in mind?)_

- Two engines to execute this metadata:

  - A fast, in-memory [implementation](./Engine)

  - A durable, reliable [implementation](./DurableTask) based on the [Durable Task framework](https://github.com/Azure/durabletask)

- Minimal external library dependencies

- Abstractions for both [synchronous](./Common/Model/Execution/IQueryMetadata.cs) and [asynchronous](./Common/Model/Execution/ISendMessageMetadata.cs) communication to external systems; talk to all your favorite native cloud services from within your statechart!

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

var interpreter = new Interpreter();

await interpreter.RunAsync(context);
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

var interpreter = new Interpreter();

await interpreter.RunAsync(context);
```

### Durable (fault-tolerant) execution

```csharp
var machine = GetMachine();

var emulator = new LocalOrchestrationService();

var service = new DurableStateChartService(emulator, machine);

await service.StartAsync();

var client = new DurableStateChartClient(emulator, machine.Id);

await client.InitAsync();

await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

await service.StopAsync();
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

var interpreter = new Interpreter();

await interpreter.RunAsync(context);
```

## Background and Resources

- Excellent, pragmatic statechart explainer [here](https://statecharts.github.io/)

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- My colleague David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts
