using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses.Market.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Market.Backtest;

[ExcludeFromCodeCoverage]
public class GetBacktestResultRequest : BaseRequest, IRequest<OperationResult<GetBacktestResultResponse>>
{
    public string Id { get; set; }
}
