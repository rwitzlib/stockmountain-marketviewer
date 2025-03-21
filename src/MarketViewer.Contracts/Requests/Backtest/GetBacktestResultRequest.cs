using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class GetBacktestResultRequest : BaseRequest, IRequest<OperationResult<GetBacktestResultResponse>>
{
    public string Id { get; set; }
}
