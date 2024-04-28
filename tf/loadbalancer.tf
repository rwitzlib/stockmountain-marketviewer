resource "aws_security_group" "alb_sg" {
  vpc_id = data.aws_vpc.stockmountain.id
  name   = "application_elb_sg"
}

resource "aws_security_group_rule" "alb_sg_ingress" {
  for_each = toset(local.alb_ingress_ports)

  security_group_id = aws_security_group.alb_sg.id
  type              = "ingress"
  from_port         = each.value
  to_port           = each.value
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_lb" "alb" {
  name               = "${local.service_name}-${var.environment}-alb"
  load_balancer_type = "application"
  subnets            = data.aws_subnets.public.ids
  idle_timeout       = 60
  security_groups    = [aws_security_group.alb_sg.id]
}

resource "aws_lb_target_group" "blue" {
  name        = "${local.service_name}-${var.environment}-blue"
  port        = 8080
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = data.aws_vpc.stockmountain.id

  health_check {
    matcher = "200,301,302,404"
    path    = "/"
  }
}

resource "aws_lb_target_group" "green" {
  name        = "${local.service_name}-${var.environment}-green"
  port        = 8080
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = data.aws_vpc.stockmountain.id

  health_check {
    matcher = "200,301,302,404"
    path    = "/"
  }
}

resource "aws_alb_listener" "blue" {
  load_balancer_arn = aws_lb.alb.id
  port              = 8080
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.blue.arn
  }
}

resource "aws_alb_listener" "green" {
  load_balancer_arn = aws_lb.alb.id
  port              = 8081
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.green.arn
  }
}