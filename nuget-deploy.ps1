
$key = ''

dotnet nuget push .\Engine\bin\Release\DurableStateMachines.Core.0.1.5.nupkg --api-key $key --source https://api.nuget.org/v3/index.json

dotnet nuget push .\FunctionHost\bin\Release\DurableStateMachines.Functions.0.1.5.nupkg --api-key $key --source https://api.nuget.org/v3/index.json

dotnet nuget push .\FunctionClient\bin\Release\DurableStateMachines.Client.0.1.5.nupkg --api-key $key --source https://api.nuget.org/v3/index.json
