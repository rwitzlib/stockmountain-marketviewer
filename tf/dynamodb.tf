resource "aws_dynamodb_table" "backtest" {
  name           = "${var.team}-${var.environment}-${var.service_name}-backtest-store"
  billing_mode   = "PROVISIONED"
  read_capacity  = 2
  write_capacity = 2
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
    name = "Parameters"
    type = "S"
  }

  global_secondary_index {
    name            = "UserIndex"
    hash_key        = "UserId"
    write_capacity  = 2
    read_capacity   = 2
    projection_type = "ALL"
  }

  global_secondary_index {
    name            = "ParametersIndex"
    hash_key        = "Parameters"
    write_capacity  = 2
    read_capacity   = 2
    projection_type = "ALL"
  }
}

resource "aws_dynamodb_table" "user" {
  name           = "${var.team}-${var.environment}-${var.service_name}-user-store"
  billing_mode   = "PROVISIONED"
  read_capacity  = 2
  write_capacity = 2
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "IsPublic"
    type = "S"
  }

  global_secondary_index {
    name            = "PublicIndex"
    hash_key        = "IsPublic"
    write_capacity  = 2
    read_capacity   = 2
    projection_type = "ALL"
  }
}