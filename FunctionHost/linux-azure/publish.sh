#!/bin/bash

az login

echo
az account list --query "[*].name" --output tsv

echo
read -p 'Enter a target subscription: ' az_sub

az account set -s $az_sub

echo
read -p 'Enter a base resource name: ' FUNCTION_APP_NAME

echo
read -p 'Enter a resource location: ' FUNCTION_APP_LOCATION

echo

az group create \
	-n "$FUNCTION_APP_NAME-rg" \
	-l $FUNCTION_APP_LOCATION

az storage account create \
	-g "$FUNCTION_APP_NAME-rg" \
	-n "${FUNCTION_APP_NAME}storage" \
	-l $FUNCTION_APP_LOCATION \
	--sku Standard_LRS

storageConnectionString=$(az storage account show-connection-string -n "${FUNCTION_APP_NAME}storage" -g "$FUNCTION_APP_NAME-rg" --output tsv)

az functionapp create \
	-g "$FUNCTION_APP_NAME-rg" \
	-n "$FUNCTION_APP_NAME-function-app" \
	-s "${FUNCTION_APP_NAME}storage" \
	--consumption-plan-location $FUNCTION_APP_LOCATION \
	--functions-version 3 \
	--os-type Linux \
	--runtime custom

cd /func-app

zip -r func.zip .

az functionapp deployment source config-zip \
	-g "$FUNCTION_APP_NAME-rg" \
	-n "$FUNCTION_APP_NAME-function-app" \
	--src ./func.zip

az functionapp config appsettings set \
	-g "$FUNCTION_APP_NAME-rg" \
	-n "$FUNCTION_APP_NAME-function-app" \
	--settings "SCDN_STORAGE_CONNECTION_STRING=$storageConnectionString" \
			   "SCDN_HUB_NAME=$FUNCTION_APP_NAME" \
			   "SCDN_CALLBACK_URI="
