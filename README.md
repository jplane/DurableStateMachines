# StateChartsDotNet

Bringing the power of hierarchical state machines to a .NET Core runtime near you.

## Background

- the [SCXML](https://www.w3.org/TR/scxml/) statecharts spec

- my colleague David Khourshid's excellent Javascript statechart library [XState](https://github.com/davidkpiano/xstate) (with fabulous [docs](https://xstate.js.org/docs/))

- nerd out on David Harel's original [research](https://www.sciencedirect.com/science/article/pii/0167642387900359/pdf) that formalized statecharts


## Goals and Future Direction

- adherence to the spec
	- but ultimately support other formats beyond XML :-)

- portable core runtime that goes wherever .NET Core goes (minimal/no external dependencies)

- useful abstractions to drive host-specific integration

	- _data model storage_ - Read/write of named data locations is formally defined in statecharts, let's store that data in interesting places
		- Azure Blob Storage
		- Azure Table Storage
		- SQL database
		- CosmosDB
		- Azure Key Vault (for secrets)
		- etc.

	- _statechart metadata storage_ - The statechart data model abstractions are defined [here](./CoreEngine.Abstractions/Model), the SCXML provider implementation is [here](./CoreEngine.ModelProvider.Xml). Where else can we store it? (see above)

	- _external service invocation_ - statecharts [formalize](https://www.w3.org/TR/scxml/#invoke) external service communication, we can define some concrete supported implementations
		- HTTP/webhook
		- dynamic assembly+type+method invocation (good for in-proc 'services')

	- _external message passing_ - again, this is [formalized](https://www.w3.org/TR/scxml/#send) in the spec and we can create several interesting implementations
		- Azure Event Grid
		- Azure Event Hub
		- Apache Kafka
		- Azure Storage Queues
		- in-proc queues

	- _work scheduling_ - support pause/resume semantics, distributed execution, etc.
		- the implementation is Task-aware which gives us the opportunity to use interesting [Task schedulers](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=netcore-3.1)

	- _visualization/debugging hooks_
		- maybe figure out how to support David's awesome [XState Viz](https://xstate.js.org/viz/)

	- _interesting host environments_
		- Azure Functions [custom runtime host](https://docs.microsoft.com/en-us/azure/azure-functions/functions-custom-handlers)
		- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
		- ???
