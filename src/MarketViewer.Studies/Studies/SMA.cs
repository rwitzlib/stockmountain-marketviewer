using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Studies.Studies;

public class SMA : IStudy
{
    public List<List<LineEntry>> Compute(string[] parameters, ref StocksResponse stocksResponse)
    {
        var series = new List<LineEntry>();
        
        if (!Validate(parameters, ref stocksResponse, out var weight))
        {
            return [];
        }

        for (int i = 0; i < stocksResponse.Results.Count; i++)
        {
            if (i < weight - 1)
            {
                continue;
            }

            var value = stocksResponse.Results.GetRange(i - (weight - 1), weight).Sum(q => q.Close) / weight;

            series.Add(new LineEntry
            {
                Timestamp = stocksResponse.Results[i].Timestamp,
                Value = value,
            });
        }

        return [series];
    }

    private static bool Validate(IReadOnlyList<object> parameters, ref StocksResponse stocksResponse, out int weight)
    {
        weight = 0;

        if (parameters is null || parameters.Count != 1)
        {
            return false;
        }

        if (int.TryParse(parameters[0].ToString(), out var _weight))
        {
            weight = _weight;
        }
        else
        {
            return false;
        }

        if (weight < 1 || weight > 1000)
        {
            return false;
        }

        if (stocksResponse.Results is not null && stocksResponse.Results.Count < weight)
        {
            return false;
        }

        return true;
    }
}
