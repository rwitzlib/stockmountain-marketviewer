data "aws_ecr_image" "marketviewer" {
  repository_name = "${local.team}-${var.environment}-${local.product}-${local.service_name}"
  image_tag       = var.image_tag
}

data "aws_ssm_parameter" "polygon_token" {
  name = "/tokens/polygon"
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