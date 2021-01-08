#!/bin/bash

# build web API app
dotnet publish "/src/app/WebHost/WebHost.csproj" -c Release -o /func-app/app

# login to Azure CLI
hideme=$(az login --service-principal --username $AZURE_CLIENT_ID --password $AZURE_CLIENT_SECRET --tenant $AZURE_TENANT_ID)

az account set -s $AZURE_SUBSCRIPTION_ID

# create Azure infrastructure
cd /src/tf

terraform init

terraform \
	-var "name=$FUNCTION_APP_NAME" \
	-var "location=$FUNCTION_APP_LOCATION" \
	-var "hubName=$SCDN_HUB_NAME" \
	-var "callbackUri=$SCDN_CALLBACK_URI" \
	apply

# publish function app + web API to Azure
cd /func-app

func azure functionapp publish $FUNCTION_APP_NAME
