# SCDN Hosted in a Linux Function app (consumption plan)

Use this to deploy and host durable statechart execution in an Azure function app (Linux consumption plan). Deployment runs interactively, prompting you for Azure login and relevant arguments. Docker is used only for deployment, not execution.

## Pre-requisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Powershell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1)
- An Azure subscription
- [Postman](http://www.postman.com) or cURL or similar HTTP testing tool
- clone this repo

## Steps

1. open a Powershell prompt to this folder and run [deploy.ps1](./deploy.ps1)
1. follow the prompts for interactive Azure login and script inputs
1. using Postman or similar, send requests to https://NAME-function-app.azurewebsites.net (see [here](../../WebHost/statecharts.postman_collection.json) for Postman request examples)
