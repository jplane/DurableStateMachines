
terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "2.42.0"
    }
  }
  backend "local" {}
}

provider "azurerm" {
  features {}
}

variable "name" {
  type        = string
  default     = "scdnjosh"
}

variable "location" {
  type        = string
  default     = "eastus2"
}

variable "hubName" {
  type        = string
  default     = "scdn"
}

variable "callbackUri" {
  type        = string
  default     = ""
}

resource "azurerm_resource_group" "rg" {
  name     = "${var.name}-rg"
  location = var.location
}

resource "azurerm_storage_account" "storage" {
  name                     = "${var.name}storage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_container_registry" "acr" {
  name                     = "${var.name}registry"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  sku                      = "Basic"
  admin_enabled            = true
}

resource "null_resource" "acrimagebuildpush" {

  provisioner "local-exec" {
    command = <<EOT

        .\build-image.ps1

        docker login ${azurerm_container_registry.acr.login_server} `
	        -u ${azurerm_container_registry.acr.admin_username} `
	        -p ${azurerm_container_registry.acr.admin_password}

        docker tag scdn/function-host:v1 ${var.name}registry.azurecr.io/scdn/function-host:v1

        docker push ${var.name}registry.azurecr.io/scdn/function-host:v1

    EOT
    interpreter = ["PowerShell", "-Command"]
  }

  triggers = {
    acr_id = azurerm_container_registry.acr.id
  }
}

resource "azurerm_app_service_plan" "plan" {
  name                = "${var.name}-service-plan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Basic"
    size = "B1"
  }
}

resource "azurerm_function_app" "app" {
  name                       = "${var.name}-function-app"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  version                    = "~3"

  site_config {
    always_on         = true
    linux_fx_version  = "DOCKER|${var.name}registry.azurecr.io/scdn/function-host:v1"
  }
  
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"              = "custom"
    "SCDN_STORAGE_CONNECTION_STRING"        = azurerm_storage_account.storage.primary_connection_string
    "SCDN_HUB_NAME"                         = var.hubName
    "SCDN_CALLBACK_URI"                     = var.callbackUri
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"   = "false"
    "DOCKER_REGISTRY_SERVER_URL"            = azurerm_container_registry.acr.login_server,
    "DOCKER_REGISTRY_SERVER_USERNAME"       = azurerm_container_registry.acr.admin_username,
    "DOCKER_REGISTRY_SERVER_PASSWORD"       = azurerm_container_registry.acr.admin_password
  }

  depends_on = [
    null_resource.acrimagebuildpush
  ]
}
