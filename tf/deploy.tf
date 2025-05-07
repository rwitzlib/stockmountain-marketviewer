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
      curl -X POST 'https://management.stockmountain.io/api/deploy/start' \
        -H 'Content-Type: application/json' \
        -H "Authorization: Bearer $TOKEN" \
        -d '{
          "id": "${var.run_id}",
          "environment": "${var.environment}",
          "repository": "stockmountain-marketviewer",
          "file": "deploy.docker-compose.yml",
          "image": "${data.aws_ecr_image.api.image_uri}",
          "actor": "${var.actor}"
        }' > /tmp/deploy_response.txt
      cat /tmp/deploy_response.txt | tail -n 1 > /tmp/deploy_status.txt
    EOT

    environment = {
      TOKEN = data.aws_ssm_parameter.deploy_token.value
    }

    interpreter = ["/bin/bash", "-c"]
  }
}

data "local_file" "deploy_status" {
  depends_on = [null_resource.deploy]
  filename   = "/tmp/deploy_status.txt"
}

output "deploy_status_code" {
  value = data.local_file.deploy_status.content
}