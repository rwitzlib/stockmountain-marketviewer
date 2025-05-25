using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MarketViewer.Contracts.Responses.Market.Backtest;

namespace MarketViewer.Core.Services;

public interface IBacktestRepository
{
    Task<bool> Create(BacktestRecord record, IEnumerable<BacktestLambdaResponseV3> entries);
    bool CheckForBacktestHistory(string compressedRequest, out BacktestRecord record);
    Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromLambda(BacktestRequestV3 request);
    Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromS3(BacktestRecord record);
}