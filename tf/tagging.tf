locals {
  default_tags = {
    Environment = var.environment
    Service     = local.service_name
    Repo        = "https://github.com/rwitzlib/stockmountain-marketviewer"
  }
}