
Remove-Item -Force -Recurse -ErrorAction Ignore "..\WebHost\bin"

Remove-Item -Force -Recurse -ErrorAction Ignore "..\WebHost\obj"

Remove-Item -Force -Recurse -ErrorAction Ignore ".\src"

Copy-Item -Force -Recurse "..\WebHost\" -Destination "src\WebHost"
Copy-Item -Force -Recurse "..\Common\" -Destination "src\Common"
Copy-Item -Force -Recurse "..\Engine\" -Destination "src\Engine"
Copy-Item -Force -Recurse "..\DurableEngine\" -Destination "src\DurableEngine"
Copy-Item -Force -Recurse "..\Metadata.Fluent\" -Destination "src\Metadata.Fluent"
Copy-Item -Force -Recurse "..\Metadata.Xml\" -Destination "src\Metadata.Xml"
Copy-Item -Force -Recurse "..\Metadata.Json\" -Destination "src\Metadata.Json"

docker build -t scdn/function-host:v1 .
