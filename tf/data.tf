data "aws_ecr_image" "marketviewer" {
  repository_name = "${local.team}-${var.environment}-${local.product}-${local.service_name}"
  image_tag       = var.image_tag
}

data "aws_ssm_parameter" "polygon_token" {
  name = "/tokens/polygon"
}

data "aws_vpc" "stockmountain" {
  filter {
    name   = "tag:Name"
    values = ["${local.team}-${var.environment}-${local.product}"]
  }
}

data "aws_ecs_cluster" "stockmountain" {
  cluster_name = "${local.team}-${var.environment}-${local.product}"
}

data "aws_subnets" "public" {
  filter {
    name   = "tag:Name"
    values = ["public-*"]
  }
}