resource "azurerm_resource_group" "main" {
  name = "rg-${var.app_name}-app"
  location = var.location
}