data "http" "deploy" {
  url = "https://management.stockmountain.io/api/deploy/start"
  method = "POST"

  request_headers = {
    Content-Type = "application/json"
    Authorization = "Bearer ${data.aws_ssm_parameter.deploy_token.value}"
  }

  request_body = jsonencode({
    environment  = "${var.environment}"
    repository   = "stockmountain-marketviewer"
    file = "deploy.docker-compose.yml"
    image = "100008144700.dkr.ecr.us-east-2.amazonaws.com/lad-dev-marketviewer-api:master"
    actor = "local"
  })
}

output "post_response_body" {
  value = data.http.deploy.body
}