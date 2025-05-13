resource "restapi_object" "deploy" {
  path = "/api/deploy/start"
  data = jsonencode({
    id          = var.run_id
    environment = var.environment
    repository  = "stockmountain-marketviewer"
    file        = "deploy.docker-compose.yml"
    image       = data.aws_ecr_image.api.image_uri
    actor       = var.actor
  })
}

output "deploy_response" {
  value = restapi_object.deploy.api_response
}