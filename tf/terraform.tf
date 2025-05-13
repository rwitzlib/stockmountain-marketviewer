terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.41.0"
    }

    http = {
      source  = "hashicorp/http"
      version = "~> 3.0"
    }

    restapi = {
      source  = "Mastercard/restapi"
      version = "2.0.1"
    }
  }

  backend "s3" {
    bucket = "lad-dev-terraform-state"
    key    = "marketviewer.tfstate"
    region = "us-east-2"
  }
}

provider "aws" {
  region = var.region

  default_tags {
    tags = local.default_tags
  }
}

provider "restapi" {
  uri                  = "https://management.stockmountain.io/api/deploy/start"
  write_returns_object = true
  debug                = true

  headers = {
    "Authorization" = "Bearer ${data.aws_ssm_parameter.deploy_token.value}",
    "Content-Type"  = "application/json"
  }

  create_method  = "PUT"
  update_method  = "PUT"
  destroy_method = "PUT"
}