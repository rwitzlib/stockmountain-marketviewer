using MarketViewer.Contracts.Dtos;

namespace MarketViewer.Core.Services;

public interface IStrategyRepository
{
    Task<StrategyDto> Put(StrategyDto strategy);
    Task<StrategyDto> Get(string id);
    Task<IEnumerable<StrategyDto>> ListByUser(string userId = null);
    Task<IEnumerable<StrategyDto>> ListByPublic(bool isPublic);
    Task<bool> Delete(string id);
}