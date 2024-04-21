using MarketViewer.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies
{
    public class SimpleMovingAverage : Study<SimpleMovingAverage>
    {
        private static int Weight { get; set; }
        
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
            if (candleData.Length < Weight)
            {
                ErrorMessages.Add("Not enough candle data.");
                return null;
            }

            var series = new List<LineEntry>();

            // Calculate SMA Series
            for (var i = Weight - 1; i < candleData.Length; i++)
            {
                float movingAvgValue = 0;
                for (var j = 0; j < Weight; j++)
                {
                    movingAvgValue += candleData[i - j].Close;
                }
                movingAvgValue /= Weight;

                series.Add(new LineEntry
                {
                    Timestamp = candleData[i].Timestamp,
                    Value = movingAvgValue,
                });
            }
            
            return
            [
                series
            ];
        }
    }
}
