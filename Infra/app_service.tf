resource "azurerm_service_plan" "main" {
  name = "asp-${var.app_name}"
  resource_group_name = azurerm_resource_group.main.name
  location = azurerm_resource_group.main.location
  os_type = "Linux"
  sku_name = "F1" # Free tier
}

resource "azurerm_linux_web_app" "main" {
  name = "${var.app_name}-api"
  resource_group_name = azurerm_resource_group.main.name
  location = azurerm_resource_group.main.location
  service_plan_id = azurerm_service_plan.main.id

  site_config {
    always_on = false # must be false on Free tier
    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    ASPNETCORE_ENVIRONMENT = "Production"
  }
}