using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;
using MarketViewer.Web.Contracts.Studies;
using MarketViewer.Web.Enums;

namespace MarketViewer.Web.Studies
{
    public class Macd : Study
    {
        private int FastWeight { get; set; }
        private int SlowWeight { get; set; }
        private int SignalWeight { get; set; }
        private MovingAverageType MovingAverageType { get; set; }
        private int BaseLineValue { get; set; } = 0;

        public Macd(MacdParams parameters)
        {
            Id = Guid.NewGuid().ToString();
            Title = $"MACD ({parameters.FastWeight},{parameters.SlowWeight},{parameters.SignalWeight},{parameters.MovingAverageType.ToString().ToUpper()})";
            Description = "Moving Average Convergence/Divergence";
            Lines = new List<Line>
            {
                new()
                {
                    Color = parameters.MacdColor.ToLowerInvariant(),
                    Width = parameters.MacdWidth,
                },
                new()
                {
                    Color = parameters.SignalColor.ToLowerInvariant(),
                    Width = parameters.SignalWidth,
                },
                new()
                {
                    Color = parameters.BaseLineColor.ToLowerInvariant(),
                    Width = parameters.BaseLineWidth,
                }
            };
            FastWeight = parameters.FastWeight;
            SlowWeight = parameters.SlowWeight;
            SignalWeight = parameters.SignalWeight;
            MovingAverageType = parameters.MovingAverageType;
            SavedParameters = parameters;
            Pane = parameters.Pane;
        }

        /// <summary>
        /// Calculates the Moving Average Convergence/Divergence based on https://www.investopedia.com/terms/m/macd.asp
        /// </summary>
        /// <param name="response"></param>
        public override void Compute(StocksResponse response)
        {            
            var studyData = response.Studies?.FirstOrDefault(q => q.Name == Title);

            if (studyData is not null)
            {
                Lines[0].Series = studyData.Results[0];
                Lines[1].Series = studyData.Results[1];
            }
            else
            {
                var candleData = response.Results.ToArray();
                var study = MovingAverageConvergenceDivergence.Compute(candleData, FastWeight, SlowWeight, SignalWeight, MovingAverageType);
                
                Lines[0].Series = study.Lines[0];
                Lines[1].Series = study.Lines[1];
            }
            
            Lines[2].Series = new List<LineEntry>();
            foreach (var entry in Lines[0].Series)
            {
                Lines[2].Series.Add(new LineEntry
                {
                    Timestamp = entry.Timestamp,
                    Value = BaseLineValue
                });
            }
        }
    }
}
