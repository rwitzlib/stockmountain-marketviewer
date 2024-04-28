terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.41.0"
    }
  }

  backend "s3" {
    bucket = "lad-dev-terraform-state"
    key    = "marketviewer.tfstate"
    region = "us-east-2"
  }
}