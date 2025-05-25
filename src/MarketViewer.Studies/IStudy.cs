using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses.Market;

namespace MarketViewer.Studies;

public interface IStudy
{
    public List<List<LineEntry>> Compute(string[] parameters, ref StocksResponse stocksResponse);
}
