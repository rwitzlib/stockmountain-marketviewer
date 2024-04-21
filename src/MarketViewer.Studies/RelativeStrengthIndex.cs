using MarketViewer.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class RelativeStrengthIndex : Study<RelativeStrengthIndex>
{
    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {
        if (parameters is not null)
        {
            ErrorMessages.Add("Too many parameters.");
            return false;
        }
        
        return true;
    }
    
    protected override List<List<LineEntry>> Initialize(Bar[] candleData)
    {
        var series = new List<LineEntry>();

        float previousAverageGain = 0;
        float previousAverageLoss = 0;

        for (var i = 1; i < 14; i++)
        {
            var change = candleData[i].Close - candleData[i - 1].Close;
            if (change > 0)
            {
                previousAverageGain += change;
            }
            else
            {
                previousAverageLoss += change * -1;
            }
        }

        for (var i = 14; i < candleData.Count(); i++)
        {
            var currentChange = candleData[i].Close - candleData[i - 1].Close;
            float averageGain = 0;
            float averageLoss = 0;
            if (currentChange > 0)
            {
                averageGain = (previousAverageGain * 13 + currentChange) / 14;
                averageLoss = (previousAverageLoss * 13 + 0) / 14;
            }
            else
            {
                averageGain = (previousAverageGain * 13 + 0) / 14;
                averageLoss = (previousAverageLoss * 13 + currentChange * -1) / 14;
            }

            var rs = averageGain / averageLoss;

            var rsi = 100 - 100 / (1 + rs);
            
            series.Add(new LineEntry
            {
                Value = rsi,
                Timestamp = candleData[i].Timestamp
            });

            previousAverageGain = averageGain;
            previousAverageLoss = averageLoss;
        }

        return
        [
            series
        ];
    }
}