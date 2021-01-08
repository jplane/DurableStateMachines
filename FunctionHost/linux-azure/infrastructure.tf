
provider "azurerm" {
  version = "=1.44.0"
}

variable "name" {
  type        = string
  default     = "scdn"
}

variable "location" {
  type        = string
  default     = "southcentralus"
}

variable "hubName" {
  type        = string
}

variable "callbackUri" {
  type        = string
}

resource "azurerm_resource_group" "rg" {
  name     = "${var.name}-rg"
  location = var.location
}

resource "azurerm_storage_account" "storage" {
  name                     = "${var.name}-storage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_app_service_plan" "plan" {
  name                = "${var.name}-service-plan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "FunctionApp"
  reserved            = true

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_function_app" "app" {
  name                       = "${var.name}-function-app"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  os_type                    = "Linux"
  
  app_settings {
    "SCDN_STORAGE_CONNECTION_STRING" = azurerm_storage_account.storage.primary_connection_string
    "SCDN_HUB_NAME"                  = var.hubName
    "SCDN_CALLBACK_URI"              = var.callbackUri
  }

}
