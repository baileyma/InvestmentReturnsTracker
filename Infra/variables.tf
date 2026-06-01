variable "location" {
  default = "westeurope"
}

variable "app_name" {
  default = "inv-tracker"
}

variable "db_password" {
  type = string
  sensitive = true
}