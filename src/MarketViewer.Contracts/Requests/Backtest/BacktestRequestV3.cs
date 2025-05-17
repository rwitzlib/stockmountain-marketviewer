using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestRequestV3 : BaseRequest, IRequest<OperationResult<BacktestResponseV3>>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDto Argument { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public bool IncludeSnapshot { get; set; } = false;
}
