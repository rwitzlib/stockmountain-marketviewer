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
    name = "UserId"
    type = "S"
  }

  attribute {
    name = "RequestDetails"
    type = "S"
  }

  global_secondary_index {
    name            = "UserIndex"
    hash_key        = "UserId"
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
    name = "Public"
    type = "N"
  }

  global_secondary_index {
    name            = "PublicIndex"
    hash_key        = "Public"
    write_capacity  = 1
    read_capacity   = 1
    projection_type = "ALL"
  }
}