using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Presentation.Responses;

namespace MarketViewer.Studies;

public interface IStudy
{
    public List<List<LineEntry>> Compute(string[] parameters, ref StocksResponse stocksResponse);
}
