# data "http" "deploy" {
#   url    = "https://management.stockmountain.io/api/deploy/start"
#   method = "POST"

#   request_headers = {
#     Content-Type  = "application/json"
#     Authorization = "Bearer ${data.aws_ssm_parameter.deploy_token.value}"
#   }

#   request_body = jsonencode({
#     id          = "${var.run_id}"
#     environment = "${var.environment}"
#     repository  = "stockmountain-marketviewer"
#     file        = "deploy.docker-compose.yml"
#     image       = data.aws_ecr_image.api.image_uri
#     actor       = var.actor
#   })
# }

# output "post_response_body" {
#   value = data.http.deploy.body
# }

resource "null_resource" "deploy" {
  triggers = {
    run_id      = var.run_id
    environment = var.environment
    image_uri   = data.aws_ecr_image.api.image_uri
  }

  provisioner "local-exec" {
    command = <<-EOT
      curl -X POST \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer ${data.aws_ssm_parameter.deploy_token.value}" \
      -d '{"id":"${var.run_id}","environment":"${var.environment}","repository":"stockmountain-marketviewer","file":"deploy.docker-compose.yml","image":"${data.aws_ecr_image.api.image_uri}","actor":"${var.actor}"}' \
      https://management.stockmountain.io/api/deploy/start
    EOT
  }
}

# output "deploy_id" {
#   value = var.run_id
# }