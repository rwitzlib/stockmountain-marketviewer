using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Presentation.Requests;
using MarketViewer.Contracts.Presentation.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class GetBacktestRequest : BaseRequest, IRequest<OperationResult<GetBacktestResultResponse>>
{
    public string Id { get; set; }
}
