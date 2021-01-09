
Remove-Item -Force -Recurse -ErrorAction Ignore "..\..\WebHost\bin"
Remove-Item -Force -Recurse -ErrorAction Ignore "..\..\WebHost\obj"
Remove-Item -Force -Recurse -ErrorAction Ignore ".\src"

Copy-Item -Force -Recurse "..\metadata\" -Destination "src\function-defs\metadata"
Copy-Item -Force -Recurse "..\register\" -Destination "src\function-defs\register"
Copy-Item -Force -Recurse "..\registerandstart\" -Destination "src\function-defs\registerandstart"
Copy-Item -Force -Recurse "..\start\" -Destination "src\function-defs\start"
Copy-Item -Force -Recurse "..\sendmessage\" -Destination "src\function-defs\sendmessage"
Copy-Item -Force -Recurse "..\status\" -Destination "src\function-defs\status"
Copy-Item -Force -Recurse "..\stop\" -Destination "src\function-defs\stop"
Copy-Item -Force -Recurse "..\host.json" -Destination "src\host.json"

Copy-Item -Force -Recurse "..\..\WebHost\" -Destination "src\WebHost"
Copy-Item -Force -Recurse "..\..\Common\" -Destination "src\Common"
Copy-Item -Force -Recurse "..\..\Engine\" -Destination "src\Engine"
Copy-Item -Force -Recurse "..\..\DurableEngine\" -Destination "src\DurableEngine"
Copy-Item -Force -Recurse "..\..\Metadata.Fluent\" -Destination "src\Metadata.Fluent"
Copy-Item -Force -Recurse "..\..\Metadata.Xml\" -Destination "src\Metadata.Xml"
Copy-Item -Force -Recurse "..\..\Metadata.Json\" -Destination "src\Metadata.Json"

docker build -t scdn/function-host:v1 .
