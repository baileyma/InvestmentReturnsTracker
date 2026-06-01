resource "azurerm_static_web_app" "main" {
  name = "${var.app_name}-frontend"
  resource_group_name = azurerm_resource_group.main.name
  location = "eastus2" # Static Web Apps has limited regions
  sku_tier = "Free"
  sku_size = "Free"
}