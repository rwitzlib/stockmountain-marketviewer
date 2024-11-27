resource "aws_dynamodb_table" "backtest" {
  name           = "${var.team}-${var.environment}-${var.service_name}-backtest-store"
  billing_mode   = "PROVISIONED"
  read_capacity  = 1
  write_capacity = 1
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CustomerId"
    type = "S"
  }

  attribute {
    name = "RequestDetails"
    type = "S"
  }

  global_secondary_index {
    name            = "CustomerIndex"
    hash_key        = "CustomerId"
    write_capacity  = 1
    read_capacity   = 1
    projection_type = "ALL"
  }

  global_secondary_index {
    name            = "RequestIndex"
    hash_key        = "RequestDetails"
    write_capacity  = 1
    read_capacity   = 1
    projection_type = "ALL"
  }
}


resource "aws_dynamodb_table" "user" {
  name           = "${var.team}-${var.environment}-${var.service_name}-user-store"
  billing_mode   = "PROVISIONED"
  read_capacity  = 1
  write_capacity = 1
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "ApiKey"
    type = "S"
  }

  global_secondary_index {
    name            = "ApiKeyIndex"
    hash_key        = "ApiKey"
    write_capacity  = 1
    read_capacity   = 1
    projection_type = "ALL"
  }
}

resource "aws_dynamodb_table" "trade" {
  name           = "${var.team}-${var.environment}-${var.service_name}-trade-store"
  billing_mode   = "PROVISIONED"
  read_capacity  = 1
  write_capacity = 1
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CustomerId"
    type = "S"
  }

  global_secondary_index {
    name            = "CustomerIndex"
    hash_key        = "CustomerId"
    write_capacity  = 1
    read_capacity   = 1
    projection_type = "ALL"
  }
}