locals {
  default_tags = {
    Team        = var.team
    Environment = var.environment
    Service     = var.service_name
    Repo        = "https://github.com/rwitzlib/stockmountain-marketviewer"
  }
}