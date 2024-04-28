locals {
  team         = "lad"
  product      = "stockmountain"
  service_name = "marketviewer"

  alb_ingress_ports = [
    "8080",
    "8081"
  ]
}