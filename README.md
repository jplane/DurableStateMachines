# StateChartsDotNet

[![Gitter](https://badges.gitter.im/StateChartsDotNet/community.svg)](https://gitter.im/StateChartsDotNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## What are statecharts?

The best definition comes from [here](https://statecharts.github.io/what-is-a-statechart.html):

> The primary feature of statecharts is that states can be organized in a hierarchy:  A statechart is a [state machine](https://statecharts.github.io/what-is-a-state-machine.html) where each state in the state machine may define its own subordinate state machines, called substates.  Those states can again define substates.

The utility of traditional state machines goes down as system complexity increases, due to [state explosion](https://statecharts.github.io/state-machine-state-explosion.html). Also, state machines by themselves are merely a description of behavior, not behavior itself. Statecharts (and StateChartsDotNet) address both of these limitations.

## Goals

StateChartsDotNet aims to provide a full-featured statechart implementation for [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/).

Some specific design and implementation choices:

- An [abstraction](./Common/Model) for describing statechart definitions and an initial [JSON](./Metadata.Json) implementation

- Abstractions for both [pull](./Common/Model/Execution/IQueryMetadata.cs)- and [push](./Common/Model/Execution/ISendMessageMetadata.cs)-based communication with external systems; talk to all your favorite native cloud services from within your statechart!

- In addition to parent-child state relationships _within_ a single statechart, there is also support for parent-child relationships _between_ statechart instances (execute statechart A within the context of statechart B, etc.)

## Getting Started

Statecharts support standard state machine concepts like [atomic](https://statecharts.github.io/glossary/atomic-state.html) and [parallel](https://statecharts.github.io/glossary/parallel-state.html) states, state [transitions](https://statecharts.github.io/glossary/transition.html), [actions](https://statecharts.github.io/glossary/action.html) within states, etc.

Statecharts also support advanced concepts like [history states](https://statecharts.github.io/glossary/history-state.html), [event-based transitions](https://statecharts.github.io/glossary/event.html), and [nested or compound state hierarchies](https://statecharts.github.io/glossary/compound-state.html). 

In SCDN you define statecharts using declarative metadata in JSON. See below for examples.

For advanced scenarios, you can also define your own metadata syntax and map it to primitives the SCDN engines can interpret and execute directly.

## [Azure Functions host](./DurableFunctionHost)

Run statecharts as a Durable Function [orchestration](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations?tabs=csharp), using the standard [HTTP API](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-http-api).

```
POST /runtime/webhooks/durabletask/orchestrators/statemachine-orchestration

{
    "args": {
        "x": 56
    },
    "statemachine": {
        "name": "sm1",
        "states": [
            {
                "id": "s1",
                "onentry": {
                    "content": [
                        {
                            "type": "assign",
                            "location": "y",
                            "expr": "x * 2"
                        }
                    ]
                },
                "transitions": [
                    {
                        "target": "alldone"
                    }
                ]
            },
            {
                "id": "alldone",
                "type": "final"
            }
        ]
    }
}

```

## Background and Resources

- Excellent, pragmatic statechart explainer [here](https://statecharts.github.io/)

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts
