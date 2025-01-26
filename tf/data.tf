data "aws_ecr_image" "api" {
  repository_name = "${var.team}-${var.environment}-${var.service_name}-api"
  image_tag       = var.image_tag
}

data "aws_ssm_parameter" "polygon_token" {
  name = "/tokens/polygon"
}

data "aws_ssm_parameter" "deploy_token" {
  name = "/tokens/${var.environment}/deploy"
}

# data "aws_vpc" "stockmountain" {
#   filter {
#     name   = "tag:Name"
#     values = ["${local.team}-${var.environment}-${local.product}"]
#   }
# }

# data "aws_ecs_cluster" "stockmountain" {
#   cluster_name = "${local.team}-${var.environment}-${local.product}"
# }

# data "aws_subnets" "public" {
#   filter {
#     name   = "tag:Name"
#     values = ["public-*"]
#   }
# }

# data "aws_security_group" "vpce" {
#   vpc_id = data.aws_vpc.stockmountain.id
#   name   = "vpce"
# }

# data "aws_vpc_endpoint" "ecr_api" {
#   vpc_id       = data.aws_vpc.stockmountain.id
#   service_name = "com.amazonaws.us-east-2.ecr.api"
# }

# data "aws_vpc_endpoint" "ecr_dkr" {
#   vpc_id       = data.aws_vpc.stockmountain.id
#   service_name = "com.amazonaws.us-east-2.ecr.dkr"
# }