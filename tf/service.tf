#https://medium.com/@ywg/blue-green-deployment-on-aws-ecs-with-terraform-7dfd67502b5b

resource "aws_ecs_service" "marketviewer_api" {
  name            = "${local.team}-${var.environment}-${local.product}-${local.service_name}-api"
  cluster         = "${local.team}-${var.environment}-${local.product}"
  task_definition = aws_ecs_task_definition.marketviewer_api_task.arn

  deployment_minimum_healthy_percent = 50
  deployment_maximum_percent         = 200
  health_check_grace_period_seconds  = 300
  launch_type                        = "FARGATE"
  scheduling_strategy                = "REPLICA"
  desired_count                      = 1

  force_new_deployment = true

  load_balancer {
    target_group_arn = aws_lb_target_group.blue.arn
    container_name   = "app"
    container_port   = "8080"
  }

  network_configuration {
    subnets          = data.aws_subnets.public
    security_groups  = [aws_security_group.ecs_task]
    assign_public_ip = true
  }

  depends_on = [
    aws_lb.alb
  ]

  lifecycle {
    ignore_changes = [task_definition, desired_count, load_balancer]
  }
}

resource "aws_ecs_task_definition" "marketviewer_api_task" {
  family                   = "${local.team}-${var.environment}-${local.product}-${local.service_name}"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  memory                   = 512
  cpu                      = 256
  execution_role_arn       = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([{
    name      = "app",
    image     = data.aws_ecr_image.marketviewer.image_uri,
    essential = true,
    portMappings = [
      {
        "containerPort" : 8080
      }
    ],
    logConfiguration = {
      logDriver = "awslogs"
      options = {
        awslogs-group         = aws_cloudwatch_log_group.marketviewer.name
        awslogs-stream-prefix = "ecs"
        awslogs-region        = var.region
      }
    }
  }])
}

resource "aws_cloudwatch_log_group" "marketviewer" {
  name              = "ecs/${var.environment}/${local.service_name}"
  retention_in_days = 14
}

resource "aws_security_group" "ecs_task" {
  name        = "${local.service_name}-${var.environment}-service"
  description = "Allow all traffic within the VPC"
  vpc_id      = data.aws_vpc.stockmountain.id

  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = [data.aws_vpc.stockmountain.cidr_block]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}