
# based on https://github.com/Azure/azure-functions-docker/blob/master/host/3.0/buster/amd64/dotnet/dotnet.Dockerfile

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

ARG HOST_VERSION=3.0.15277

ENV PublishWithAspNetCoreTargetManifest=false

RUN BUILD_NUMBER=$(echo ${HOST_VERSION} | cut -d'.' -f 3) && \
    git clone --branch v${HOST_VERSION} https://github.com/Azure/azure-functions-host /src/azure-functions-host && \
    cd /src/azure-functions-host && \
    HOST_COMMIT=$(git rev-list -1 HEAD) && \
    dotnet publish -v q /p:BuildNumber=$BUILD_NUMBER /p:CommitHash=$HOST_COMMIT src/WebJobs.Script.WebHost/WebJobs.Script.WebHost.csproj -c Release --output /azure-functions-host --runtime linux-x64 && \
    mv /azure-functions-host/workers /workers && mkdir /azure-functions-host/workers && \
    rm -rf /root/.local /root/.nuget /src

RUN EXTENSION_BUNDLE_VERSION=1.5.0 && \
    EXTENSION_BUNDLE_FILENAME=Microsoft.Azure.Functions.ExtensionBundle.1.5.0_linux-x64.zip && \
    apt-get update && \
    apt-get install -y gnupg wget unzip && \
    wget https://functionscdn.azureedge.net/public/ExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION/$EXTENSION_BUNDLE_FILENAME && \
    mkdir -p /FuncExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION && \
    unzip /$EXTENSION_BUNDLE_FILENAME -d /FuncExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION && \
    rm -f /$EXTENSION_BUNDLE_FILENAME && \
    EXTENSION_BUNDLE_VERSION_V2=2.4.0 && \
    EXTENSION_BUNDLE_FILENAME_V2=Microsoft.Azure.Functions.ExtensionBundle.${EXTENSION_BUNDLE_VERSION_V2}_linux-x64.zip && \
    wget https://functionscdn.azureedge.net/public/ExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION_V2/$EXTENSION_BUNDLE_FILENAME_V2 && \
    mkdir -p /FuncExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION_V2 && \
    unzip /$EXTENSION_BUNDLE_FILENAME_V2 -d /FuncExtensionBundles/Microsoft.Azure.Functions.ExtensionBundle/$EXTENSION_BUNDLE_VERSION_V2 && \
    rm -f /$EXTENSION_BUNDLE_FILENAME_V2 &&\
    find /FuncExtensionBundles/ -type f -exec chmod 644 {} \;

COPY /src /src/app

RUN dotnet publish "/src/app/WebHost/WebHost.csproj" -c Release -o /WebHost



FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    HOME=/home \
    FUNCTIONS_WORKER_RUNTIME=Custom \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    HOST_VERSION=${HOST_VERSION} \
    ASPNETCORE_URLS=http://*:80

COPY --from=build /azure-functions-host /azure-functions-host
COPY --from=build /FuncExtensionBundles /FuncExtensionBundles
COPY --from=build /WebHost /home/site/wwwroot/app

COPY --from=build /src/app/host.json /home/site/wwwroot/host.json

COPY --from=build /src/app/function-defs/metadata/function.json /home/site/wwwroot/metadata/function.json
COPY --from=build /src/app/function-defs/register/function.json /home/site/wwwroot/register/function.json
COPY --from=build /src/app/function-defs/registerandstart/function.json /home/site/wwwroot/registerandstart/function.json
COPY --from=build /src/app/function-defs/start/function.json /home/site/wwwroot/start/function.json
COPY --from=build /src/app/function-defs/status/function.json /home/site/wwwroot/status/function.json
COPY --from=build /src/app/function-defs/sendmessage/function.json /home/site/wwwroot/sendmessage/function.json
COPY --from=build /src/app/function-defs/stop/function.json /home/site/wwwroot/stop/function.json

CMD [ "/azure-functions-host/Microsoft.Azure.WebJobs.Script.WebHost" ]
