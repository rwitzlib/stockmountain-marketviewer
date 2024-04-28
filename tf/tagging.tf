locals {
  default_tags = {
    Environment = var.environment
    Product     = local.product
    Service     = local.service_name
    Repo        = "https://github.com/rwitzlib/stockmountain-marketviewer"
  }
}