using MarketViewer.Contracts.Records;
using MarketViewer.Contracts.Responses.Market.Backtest;

namespace MarketViewer.Core.Services;

public interface IBacktestRepository
{
    Task<bool> Put(BacktestRecord record, IEnumerable<WorkerResponse> entries);
    Task<BacktestRecord> Get(string id);
    Task<List<BacktestRecord>> List(string userId);
    bool CheckForBacktestHistory(string compressedRequest, out BacktestRecord record);
    Task<List<WorkerResponse>> GetBacktestResultsFromS3(BacktestRecord record);
}