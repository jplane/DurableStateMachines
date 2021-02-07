# Durable State Machines

DSM implements a declarative state machine programming model as an extension for [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp). It's based on the "hierarchical state machine" concepts behind [statecharts](https://statecharts.github.io/) and the [SCXML W3C Recommendation](https://www.w3.org/TR/scxml/).

| | |  |
| ------------- | ----------- | --- |
| DurableStateMachines.Core | Metadata + interpreter (standalone, in-memory component... no durable bits) | [![NuGet](https://img.shields.io/nuget/v/DurableStateMachines.Core)](https://www.nuget.org/packages/DurableStateMachines.Core/) |
| DurableStateMachines.Functions | Durable Functions integration | [![NuGet](https://img.shields.io/nuget/v/DurableStateMachines.Functions)](https://www.nuget.org/packages/DurableStateMachines.Functions/) |
| DurableStateMachines.Client | Durable Functions client API | [![NuGet](https://img.shields.io/nuget/v/DurableStateMachines.Client)](https://www.nuget.org/packages/DurableStateMachines.Client/) |
| | |  |


## What are statecharts?

The best definition comes from [here](https://statecharts.github.io/what-is-a-statechart.html):

> The primary feature of statecharts is that states can be organized in a hierarchy:  A statechart is a [state machine](https://statecharts.github.io/what-is-a-state-machine.html) where each state in the state machine may define its own subordinate state machines, called substates.  Those states can again define substates.

The utility of traditional state machines goes down as system complexity increases, due to [state explosion](https://statecharts.github.io/state-machine-state-explosion.html). Also, state machines by themselves are merely a description of behavior, not behavior itself. Statecharts (and DSM) address both of these limitations.

## Goals

DSM aims to provide a full-featured statechart implementation for [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/). This means you can run state machines locally on your laptop, or anywhere Azure Functions will run (Kubernetes, Azure serverless, edge compute, etc.).

Some specific design and implementation choices:

- An [abstraction](./Common/Model) for describing statechart definitions and an [initial](./Metadata) implementation

- Abstractions for both [pull](./Common/Model/Execution/IQueryMetadata.cs)- and [push](./Common/Model/Execution/ISendMessageMetadata.cs)-based communication with external systems; talk to all your favorite native cloud services from within your statechart!

- In addition to parent-child state relationships _within_ a single statechart, there is also support for parent-child relationships _between_ statechart instances (execute statechart A within the context of statechart B, etc.)

- An extensible [telemetry service](./FunctionClient/Listener.cs) implemented with [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)... observe state machine execution as it occurs, etc.

## Getting Started

Statecharts support standard state machine concepts like [atomic](https://statecharts.github.io/glossary/atomic-state.html) and [parallel](https://statecharts.github.io/glossary/parallel-state.html) states, state [transitions](https://statecharts.github.io/glossary/transition.html), [actions](https://statecharts.github.io/glossary/action.html) within states, etc.

Statecharts also support advanced concepts like [history states](https://statecharts.github.io/glossary/history-state.html), [event-based transitions](https://statecharts.github.io/glossary/event.html), and [nested or compound state hierarchies](https://statecharts.github.io/glossary/compound-state.html). 

In SCDN you define statecharts using declarative metadata in JSON. See below for examples.

For advanced scenarios, you can also define your own metadata syntax and map it to primitives the SCDN engines can interpret and execute directly.

## Usage

Run statecharts as a Durable Function [orchestration](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations?tabs=csharp), using the standard [IDurableClient](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.extensions.durabletask.idurableclient?view=azure-dotnet) APIs.

### Option 1: strongly-typed C#

```csharp
// in your DF app (on the server)

public class Definitions
{
    [StateMachineDefinition("my-state-machine")]
    public StateMachine<(int x, int y)> MyStateMachine =>
        new StateMachine<(int x, int y)>
        {
            Id = "test",
            States =
            {
                new AtomicState<(int x, int y)>
                {
                    Id = "state1",
                    OnEntry = new OnEntryExit<(int x, int y)>
                    {
                        Actions =
                        {
                            new Assign<(int x, int y)>
                            {
                                Target = d => d.x,
                                ValueFunction = data => data.x + 1
                            }
                        }
                    },
                    OnExit = new OnEntryExit<(int x, int y)>
                    {
                        Actions =
                        {
                            new Assign<(int x, int y)>
                            {
                                Target = d => d.x,
                                ValueFunction = data => data.x + 1
                            }
                        }
                    },
                    Transitions =
                    {
                        new Transition<(int x, int y)>
                        {
                            Targets = { "alldone" }
                        }
                    }
                },
                new FinalState<(int x, int y)>
                {
                    Id = "alldone"
                }
            }
        };
}

```

```csharp
// on the client

IDurableClient client = GetDurableFunctionClient();     // obtain via dependency injection

var data = (5, 0);                                      // any serializable type

var instanceId = await client.StartNewStateMachineAsync("my-state-machine", data);

var result = await client.WaitForStateMachineCompletionAsync(instanceId);

data = result.ToOutput<(int x, int y)>();

Console.WriteLine(data.x);

```

### Option 2: HTTP + JSON

```

// from any client

HTTP POST /runtime/webhooks/durabletask/orchestrators/statemachine-definition

{
  'input': {
    'items': [ 1, 2, 3, 4, 5 ],
    'sum': 0
  },
  'statemachine': {
    'id': 'test',
    'initialstate': 'loop',
    'states': [
      {
        'id': 'loop',
        'type': 'atomic',
        'onentry': {
          'actions': [
            {
              'type': 'foreach',
              'valueexpression': 'items',
              'currentitemlocation': 'arrayItem',
              'actions': [
                {
                  'type': 'assign',
                  'target': 'sum',
                  'valueexpression': 'sum + arrayItem'
                },
                {
                  'type': 'log',
                  'messageexpression': '""item = "" + arrayItem'
                }
              ]
            }
          ]
        },
        'transitions': [
          {
            'conditionexpression': 'sum >= 15',
            'target': 'done'
          }
        ]
      },
      {
        'id': 'done',
        'type': 'final',
        'onentry': {
          'actions': [
            {
              'type': 'log',
              'messageexpression': '""item = "" + arrayItem'
            }
          ]
        }
      }
    ]
  }
}

```

## Samples and Tests

- Unit tests are [here](./Tests) (work-in-progress :-))

- Small standalone durable tests are [here](./DurableTests)

- More full-featured sample applications are [here](./Samples)

## Background and Resources

- Excellent, pragmatic statechart explainer [here](https://statecharts.github.io/)

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts
