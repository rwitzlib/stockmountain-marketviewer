using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;
using MarketViewer.Web.Contracts.Studies;

namespace MarketViewer.Web.Studies
{
    public class Vwap : Study
    {
        private string Color { get; set; }

        public Vwap(VwapParams parameters)
        {
            Id = Guid.NewGuid().ToString();
            Title = "VWAP";
            Description = "Volume Weighted Average Price";
            Lines = new List<Line>()
            {
                new Line
                {
                    Color = parameters.Color.ToLowerInvariant(),
                    Width = parameters.Width
                }
            };
            Color = parameters.Color.ToLowerInvariant();
            SavedParameters = parameters;
            Pane = parameters.Pane;
        }

        /// <summary>
        /// Calculates a volume weighted average price based on https://github.com/polygon-io/issues/issues/131#issuecomment-927917108
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Simple Moving Average</returns>
        public override void Compute(StocksResponse response)
        {
            var studyData = response.Studies?.FirstOrDefault(q => q.Name.Equals(Title, StringComparison.InvariantCultureIgnoreCase));
            
            if (studyData is not null)
            {
                Lines[0].Series = studyData.Results[0];
            }
            else
            {
                var candleData = response.Results;
                Lines[0].Series = VolumeWeightedAveragePrice.Compute(candleData, null).Lines[0];
            }
        }
    }
}
