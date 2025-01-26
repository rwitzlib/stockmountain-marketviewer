data "http" "deploy" {
  url    = "https://management.stockmountain.io/api/deploy/start"
  method = "POST"

  request_headers = {
    Content-Type  = "application/json"
    Authorization = "Bearer ${data.aws_ssm_parameter.deploy_token.value}"
  }

  request_body = jsonencode({
    id          = "${var.run_id}"
    environment = "${var.environment}"
    repository  = "stockmountain-marketviewer"
    file        = "deploy.docker-compose.yml"
    image       = data.aws_ecr_image.api.image_uri
    actor       = var.actor
  })
}

output "post_response_body" {
  value = data.http.deploy.body
}