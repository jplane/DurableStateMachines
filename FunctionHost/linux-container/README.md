# SCDN Azure Functions host in a Linux Container

## Local

#### Pre-requisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Powershell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1)
- An Azure [storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) or [local emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)
- [Postman](http://www.postman.com) or cURL or similar HTTP testing tool
- clone this repo

#### Steps

1. edit [run-local.ps1](./run-local.ps1) and add your storage account connection string, and a unique hub name (hub name identifies your application)
1. open a Powershell prompt to this folder and run [build-image.ps1](./build-image.ps1)
1. run [run-local.ps1](./run-local.ps1)
1. using Postman or similar, send requests to http://localhost:5002 (see [here](../../WebHost/statecharts.postman_collection.json) for Postman request examples)

## In Azure

This creates an Azure resource group containing:

- an Azure Container Registry
- a storage account
- a container-based function app running on a 'Basic:B1' app service plan

#### Pre-requisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Powershell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and a valid subscription
- [Terraform](https://learn.hashicorp.com/tutorials/terraform/install-cli)
- [Postman](http://www.postman.com) or cURL or similar HTTP testing tool
- clone this repo

#### Steps

1. open a Powershell prompt to this folder and run `terraform init`
1. now run `terraform apply` (optional: pass [variables](https://www.terraform.io/docs/commands/apply.html#var-39-foo-bar-39-) for 'name', 'location', and 'hubName' to override the defaults in the [script](./deploy-to-azure.tf))
1. using Postman or similar, send requests to https://NAME-function-app.azurewebsites.net (see [here](../../WebHost/statecharts.postman_collection.json) for Postman request examples)
