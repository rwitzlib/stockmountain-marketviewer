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

variable "service_name" {
  type    = string
  default = "marketviewer"
}

variable "team" {
  type    = string
  default = "lad"
}

variable "repository_name" {
  type    = string
  default = "stockmountain-marketviewer"
}

variable "actor" {
  type    = string
  default = "local"
}

variable "run_id" {
  type    = string
  default = "12345"
}