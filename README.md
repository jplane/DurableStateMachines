# StateChartsDotNet

[![Gitter](https://badges.gitter.im/StateChartsDotNet/community.svg)](https://gitter.im/StateChartsDotNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Bringing the power of hierarchical state machines to a .NET Core runtime near you.

## Background

- The [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- My colleague David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- Nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts

## Goals and Future Direction

- Adherence to the spec
	- But ultimately support other formats beyond XML :-)

- Portable core runtime that goes wherever .NET Core goes (minimal/no external dependencies)

- Useful abstractions to drive host-specific integration

	- __External service invocation__ - Statecharts [formalize](https://www.w3.org/TR/scxml/#invoke) external service communication, we can define some concrete supported implementations
		- HTTP/webhook
		- Dynamic assembly+type+method invocation (good for in-proc 'services')

	- __External message passing__ - Again, this is [formalized](https://www.w3.org/TR/scxml/#send) in the spec and we can create several interesting implementations
		- Azure Event Grid
		- Azure Event Hub
		- Apache Kafka
		- Azure Storage Queues
		- in-proc queues

	- __Visualization/debugging hooks__
		- Define a lightweight debugger protocol to enable breakpoints, state inspection, etc.
		- Maybe figure out how to support David's awesome [XState Viz](https://xstate.js.org/viz/)

	- __Interesting host environments__
		- Azure Functions [custom runtime host](https://docs.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers)
		- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
		- ???
