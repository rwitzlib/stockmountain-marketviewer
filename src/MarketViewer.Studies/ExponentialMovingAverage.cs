using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;
using System.Reflection.Metadata.Ecma335;

namespace MarketViewer.Studies;

public class ExponentialMovingAverage : Study<ExponentialMovingAverage>
{
    private static int Weight { get; set; }

    #region Protected Methods

    protected override List<List<LineEntry>> Initialize(Bar[] candles)
    {
        var series = new List<LineEntry>();
        
        if (candles.Length < Weight)
        {
            ErrorMessages.Add("Not enough candle data.");
            return [series];
        }

        for (int i = 0; i < candles.Length; i++)
        {
            if (i < Weight - 1)
            {
                continue;
            }

            var value = GetExponentialMovingAverage(candles, series, i, Weight);

            series.Add(new LineEntry
            {
                Timestamp = candles[i].Timestamp,
                Value = value
            });
        }

        return [ series ];
    }

    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {        
        if (parameters?.Count != 1)
        {
            ErrorMessages.Add("Too many parameters.");
            return false;
        }

        if (int.TryParse(parameters[0].ToString(), out var weight))
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

    #endregion

    #region Private Methods

    private static float GetSimpleMovingAverage(IEnumerable<Bar> candles, int index, int weight)
    {
        var value = candles.ToList().GetRange(index - (weight - 1), weight).Sum(q => q.Close) / weight;

        return value;
    }

    private static float GetExponentialMovingAverage(IEnumerable<Bar> candles, List<LineEntry> series, int index, int weight)
    {
        if (!series.Any())
        {
            return GetSimpleMovingAverage(candles, index, weight);
        }

        var smoothingFactor = 2f / (weight + 1);

        var value = (candles.ToArray()[index].Close * smoothingFactor) + (series.Last().Value * (1 - smoothingFactor));

        return value;
    }

    #endregion
}