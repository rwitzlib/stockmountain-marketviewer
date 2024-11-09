using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class SimpleMovingAverage : Study<SimpleMovingAverage>
{
    private static int Weight { get; set; }

    #region Protected Methods

    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {        
        if (parameters?.Count > 1)
        {
            ErrorMessages.Add("Too many parameters.");
            return false;
        }

        if (int.TryParse(parameters?[0].ToString(), out var weight))
        {
            Weight = weight;
        }
        else
        {
            ErrorMessages.Add("Parameter must be an integer.");
            return false;
        }

        return true;
    }

    protected override List<List<LineEntry>> Initialize(Bar[] candleData)
    {
        var series = new List<LineEntry>();

        if (candleData.Length < Weight)
        {
            ErrorMessages.Add("Not enough candle data.");
            return [series];
        }

        for (int i = 0; i < candleData.Length; i++)
        {
            if (i < Weight - 1)
            {
                continue;
            }

            var value = GetSimpleMovingAverage(candleData, i, Weight);

            series.Add(new LineEntry
            {
                Timestamp = candleData[i].Timestamp,
                Value = value,
            });
        }
        
        return
        [
            series
        ];
    }

    #endregion

    #region Private Methods

    private static float GetSimpleMovingAverage(IEnumerable<Bar> candles, int index, int weight)
    {
        var value = candles.ToList().GetRange(index - (weight - 1), weight).Sum(q => q.Close) / weight;

        return value;
    }

    #endregion
}
