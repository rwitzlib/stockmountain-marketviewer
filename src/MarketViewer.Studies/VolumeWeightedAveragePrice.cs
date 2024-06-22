using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class VolumeWeightedAveragePrice : Study<VolumeWeightedAveragePrice>
{
    protected override List<List<LineEntry>> Initialize(Bar[] candleData)
    {
        var series = new List<LineEntry>();

        float cumulativeVolume = 0;
        float cumulativeDollarVolume = 0;
        var currentDay = DateTimeOffset.FromUnixTimeMilliseconds(candleData.First().Timestamp).Day;

        foreach (var candle in candleData)
        {
            // Reset VWAP calculation at beginning of each day
            if (DateTimeOffset.FromUnixTimeMilliseconds(candle.Timestamp).Day > currentDay)
            {
                cumulativeVolume = 0;
                cumulativeDollarVolume = 0;
                currentDay = DateTimeOffset.FromUnixTimeMilliseconds(candle.Timestamp).Day;
            }

            cumulativeVolume += candle.Volume;
            cumulativeDollarVolume += candle.Vwap * candle.Volume;

            var vwap = cumulativeDollarVolume / cumulativeVolume;

            series.Add(new LineEntry
            {
                Timestamp = candle.Timestamp,
                Value = vwap
            });
        }

        return
        [
            series
        ];
    }

    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {
        if (parameters is null || parameters.Count == 0)
        {
            return true;
        }
        
        ErrorMessages.Add("Too many parameters.");
        return false;
    }
}