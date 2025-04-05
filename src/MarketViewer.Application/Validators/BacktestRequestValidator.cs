using FluentValidation;
using MarketViewer.Contracts.Presentation.Requests.Backtest;
using System;

namespace MarketViewer.Application.Validators
{
    public class BacktestRequestValidator : AbstractValidator<BacktestRequestV3>
    {
        public BacktestRequestValidator()
        {
            //RuleFor(param => param.Start).LessThanOrEqualTo(param => param.End).GreaterThan(param => param.Start.AddYears(-15));
            //RuleFor(param => param.Start.ToString("yyyy-MM-dd")).LessThanOrEqualTo(DateTimeOffset.Now.ToString("yyyy-MM-dd"));

            //RuleFor(param => param.End).GreaterThanOrEqualTo(param => param.Start).GreaterThan(param => param.Start.AddYears(-15));
            //RuleFor(param => param.End.ToString("yyyy-MM-dd")).LessThanOrEqualTo(DateTimeOffset.Now.ToString("yyyy-MM-dd"));

            //RuleFor(param => param.Argument).NotNull();
        }
    }
}
