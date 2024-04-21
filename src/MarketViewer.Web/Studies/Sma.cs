using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;
using MarketViewer.Web.Contracts.Studies;

namespace MarketViewer.Web.Studies
{
    public class Sma : Study
    {
        private int SimpleMovingAverageWeight { get; set; }
        private string Color { get; set; }

        public Sma(SmaParams parameters)
        {
            Id = Guid.NewGuid().ToString();
            Title = $"{parameters.Weight} SMA";
            Description = "Simple Moving Average";
            Lines = new List<Line>
            {
                new()
                {
                    Color = parameters.Color.ToLowerInvariant(),
                    Width = parameters.Width
                }
            };
            Color = parameters.Color.ToLowerInvariant();
            SimpleMovingAverageWeight = parameters.Weight;
            SavedParameters = parameters;
            Pane = parameters.Pane;
        }

        /// <summary>
        /// Calculates a simple moving average based on https://corporatefinanceinstitute.com/resources/equities/simple-moving-average-sma/
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Simple Moving Average</returns>
        public override void Compute(StocksResponse response)
        {
            var studyData = response.Studies?.FirstOrDefault(q => q.Name == Title);
            
            if (studyData is not null)
            {            
                Lines[0].Series = studyData.Results[0];
            }
            else
            {
                var candleData = response.Results.ToArray();
                Lines[0].Series = SimpleMovingAverage.Compute(candleData, SimpleMovingAverageWeight).Lines[0];
            }
        }
    }
}
