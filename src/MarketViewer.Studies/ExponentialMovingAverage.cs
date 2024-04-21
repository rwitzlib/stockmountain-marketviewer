using MarketViewer.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class ExponentialMovingAverage : Study<ExponentialMovingAverage>
{
    private static int Weight { get; set; }

    protected override List<List<LineEntry>> Initialize(Bar[] candleData)
    {
        if (candleData.Length < Weight)
        {
            ErrorMessages.Add("Not enough candle data.");
            return null;
        }

        var series = new List<LineEntry>();

        // Get Initial SMA
        float simpleMovingAverage = 0;
        for (var i = 0; i < Weight; i++)
        {
            simpleMovingAverage += candleData[i].Close;
        }
        simpleMovingAverage /= Weight;

        // Set Initial SMA as first EMA
        series.Add(new LineEntry
        {
            Timestamp = candleData[Weight - 1].Timestamp,
            Value = simpleMovingAverage
        });

        // Calculate Constant
        var weightFactor = 2f / (Weight + 1);

        // Calculate EMA series
        for (var i = Weight; i < candleData.Length; i++)
        {
            var previousExponentialMovingAvg = series[^1].Value;

            var currentExponentialMovingAvg = weightFactor * (candleData[i].Close - previousExponentialMovingAvg) + previousExponentialMovingAvg;

            series.Add(new LineEntry
            {
                Timestamp = candleData[i].Timestamp,
                Value = currentExponentialMovingAvg
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
}