resource "azurerm_mssql_server" "main" {
  name                         = "${var.app_name}-sqlserver"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = "northeurope"
  version                      = "12.0"
  administrator_login = "CloudSA2df448e6"
  administrator_login_password = var.db_password
}

resource "azurerm_mssql_database" "main" {
  name      = "investment-tracker-db"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "GP_S_Gen5_1"
  
  auto_pause_delay_in_minutes = 60
  min_capacity                = 0.5
  storage_account_type = "Local"
}

resource "azurerm_mssql_firewall_rule" "azure" {
  name             = "allow-azure-services"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
