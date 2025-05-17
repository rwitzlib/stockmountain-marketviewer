using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class GetBacktestRequest : BaseRequest, IRequest<OperationResult<GetBacktestResultResponse>>
{
    public string Id { get; set; }
}
