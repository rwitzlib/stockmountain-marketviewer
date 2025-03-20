using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies.Studies;
using MarketViewer.Web.Contracts.Studies;

namespace MarketViewer.Web.Studies
{
    public class Ema : Study
    {
        private int ExponentialMovingAverageWeight { get; set; }
        private string Color { get; set; }

        public Ema(EmaParams parameters)
        {
            Id = Guid.NewGuid().ToString();
            Title = $"EMA ({parameters.Weight})";
            Description = "Exponential Moving Average";
            Lines = new List<Line>
            {
                new()
                {
                    Color = parameters.Color.ToLowerInvariant(),
                    Width = parameters.Width
                }
            };
            Color = parameters.Color.ToLowerInvariant();
            ExponentialMovingAverageWeight = parameters.Weight;
            SavedParameters = parameters;
            Pane = parameters.Pane;
        }

        /// <summary>
        /// Calculates a exponential moving average based on https://corporatefinanceinstitute.com/resources/equities/exponential-moving-average-ema/
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Exponential Moving Average</returns>
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
                Lines[0].Series = EMA.Compute(candleData, ExponentialMovingAverageWeight).Lines[0];
            }
        }
    }
}
