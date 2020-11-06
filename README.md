# StateChartsDotNet

Bringing the power of hierarchical state machines to a .NET Core runtime near you.

## Background

- the [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- my colleague David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts


## Goals and Future Direction

- adherence to the spec
	- but ultimately support other formats beyond XML :-)

- portable core runtime that goes wherever .NET Core goes (no external dependencies)

- useful abstractions to drive environment-specific integration
	- data model storage
	- statechart metadata storage
	- external service invocation
	- incoming/outgoing message passing
	- incoming/outgoing event handling
	- logging and telemetry
	- handling secrets
	- work scheduling to support pause/resume semantics, distributed execution, etc.
	- visualization/debugging hooks (maybe figure out how to support David's awesome [XState Viz](https://xstate.js.org/viz/))
