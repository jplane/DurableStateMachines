
FROM mcr.microsoft.com/azure-functions/dotnet:3.0-dotnet3-core-tools

RUN apt-get update && apt-get install -y zip jq

COPY /src/function-defs /func-app
COPY /src/host.json /func-app/host.json
COPY /src/proxies.json /func-app/proxies.json
COPY /src/publish.sh /publish.sh

COPY /src/app /src

RUN dotnet publish "/src/WebHost/WebHost.csproj" -r linux-x64 -p:PublishSingleFile=true --self-contained true -c Release -o /func-app/app

CMD ["/publish.sh"]
