using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies.Studies;
using MarketViewer.Web.Contracts.Studies;

namespace MarketViewer.Web.Studies
{
    public class Rsi : Study
    {
        private int OverboughtLevel { get; set; }
        private string OverBoughtColor { get; set; } = "red";
        private int OversoldLevel { get; set; }
        private string OversoldColor { get; set; } = "aqua";
        private string RsiColor { get; set; } = "blue";
        private string BaseColor { get; set; } = "gray";

        public Rsi(RsiParams parameters)
        {
            Id = Guid.NewGuid().ToString();
            Title = $"RSI";
            Description = "Relative Strength Index";
            Lines = new List<Line>
            {
                new()
                {
                    Color = RsiColor,
                    Width = 1,
                },
                new()
                {
                    Color = BaseColor,
                    Width = 1,
                },
                new()
                {
                    Color = BaseColor,
                    Width = 1,
                }
            };
            OverboughtLevel = parameters.OverboughtLevel;
            OversoldLevel = parameters.OversoldLevel;
            SavedParameters = parameters;
            Pane = parameters.Pane;
        }

        /// <summary>
        /// Calculates the relative strength index based on https://www.investopedia.com/terms/r/rsi.asp | https://school.stockcharts.com/doku.php?id=technical_indicators:relative_strength_index_rsi
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
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
                Lines[0].Series = RSI.Compute(candleData).Lines[0];
            }
            
            Lines[1].Series = new List<LineEntry>();
            Lines[2].Series = new List<LineEntry>();

            foreach (var entry in Lines[0].Series)
            {
                Lines[1].Series.Add(new LineEntry
                {
                    Value = OverboughtLevel,
                    Timestamp = entry.Timestamp
                });

                Lines[2].Series.Add(new LineEntry
                {
                    Value = OversoldLevel,
                    Timestamp = entry.Timestamp
                });
            }
        }
    }
}
