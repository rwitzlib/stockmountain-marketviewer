data "aws_caller_identity" "current" {}

variable "image_tag" {
  type        = string
  description = "This is the image name that will be populated by the pipeline."
}

variable "environment" {
  type    = string
  default = "dev"
}

variable "region" {
  type    = string
  default = "us-east-2"
}