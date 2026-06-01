output "app_service_url" {
  value = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "sql_server_host" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "static_web_app_api_token" {
  value = azurerm_static_web_app.main.api_key
  sensitive = true
}