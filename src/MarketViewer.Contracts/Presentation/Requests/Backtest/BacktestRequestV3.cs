using MarketViewer.Contracts.Entities.Backtest;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Presentation.Models;
using MarketViewer.Contracts.Presentation.Requests;
using MarketViewer.Contracts.Presentation.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestRequestV3 : BaseRequest, IRequest<OperationResult<BacktestResponseV3>>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDetails Argument { get; set; }
}
