using MarketViewer.Contracts.Records;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MarketViewer.Contracts.Responses.Market.Backtest;

namespace MarketViewer.Core.Services;

public interface IBacktestRepository
{
    Task<bool> Put(BacktestRecord record, IEnumerable<BacktestLambdaResponseV3> entries);
    Task<BacktestRecord> Get(string id);
    Task<List<BacktestRecord>> List(string userId);
    bool CheckForBacktestHistory(string compressedRequest, out BacktestRecord record);
    Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromLambda(BacktestRequestV3 request);
    Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromS3(BacktestRecord record);
}