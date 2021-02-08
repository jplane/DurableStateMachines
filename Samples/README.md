# Samples

Included here are two full-featured samples that demonstrate various aspects of state machine usage. These samples are built against the [public](https://www.nuget.org/packages/DurableStateMachines.Functions/) [Nuget](https://www.nuget.org/packages/DurableStateMachines.Core/) [packages](https://www.nuget.org/packages/DurableStateMachines.Client/) for the DurableStateMachine project.

## Interactive Debugger

[InteractiveDebugger.sln](./InteractiveDebugger)

This sample demonstrates a simple web interface for submitting state machine execution requests as JSON. It also demonstrates the ability to listen for observability events during state machine execution, using a SignalR client. The web app pauses execution to display state machine progress, and allows the user to resume execution at the click of a button.

This sample includes a number of state machine definitions as starters; you can execute those directly or modify them in-browser as you wish.

## Strongly Typed Client

[StronglyTypedClient.sln](./StronglyTypedClient)

This sample demonstrates the use of [IDurableClient](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.extensions.durabletask.idurableclient?view=azure-dotnet) to submit state machine jobs, check status, etc. using purpose-built extension methods. IDurableClient invocations are submitted by name, and mapped to existing definitions already registered on the Durable Functions application.

It also demonstrates handling optional observability events using SignalR in a C# console client.
