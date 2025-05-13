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

    httpclient = {
      version = "0.0.3"
      source  = "dmachard/http-client"
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

provider "httpclient" {}