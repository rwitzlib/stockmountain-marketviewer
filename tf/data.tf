data "aws_ecr_image" "marketviewer" {
    repository_name = "${local.team}-${var.environment}-${local.business_domain}-${local.service_name}"
    image_tag = var.image_tag
}

data "aws_ssm_parameter" "polygon_token" {
    name = "/tokens/polygon"
}