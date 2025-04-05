using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Scan;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class StartBacktestRequest : BaseRequest, IRequest<OperationResult<StartBacktestResponse>>
{
    [JsonIgnore]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentRequest Argument { get; set; }
}
