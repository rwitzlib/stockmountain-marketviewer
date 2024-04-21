using MarketViewer.Web.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts.Studies
{
    [ExcludeFromCodeCoverage]
    public class MacdParams : StudyParams
    {
        public MacdParams()
        {
            MovingAverageType = MovingAverageType.ema;
            FastWeight = 12;
            SlowWeight = 26;
            SignalWeight = 9;

            MacdColor = "green";
            SignalColor = "red";
            BaseLineColor = "fuchsia";

            MacdWidth = 1;
            SignalWidth = 1;
            BaseLineWidth = 1;
            
            Pane = 1;
        }

        public MacdParams(IReadOnlyList<string> parameters)
        {
            if (parameters.Count != 4 ||
                !int.TryParse(parameters[0], out var fastWeight) ||
                !int.TryParse(parameters[1], out var slowWeight) ||
                !int.TryParse(parameters[2], out var signalWeight) ||
                !Enum.TryParse(parameters[3], out MovingAverageType type))
            {
                return;
            }
            
            FastWeight = fastWeight;
            SlowWeight = slowWeight;
            SignalWeight = signalWeight;
            MovingAverageType = type;
            
            MacdColor = "green";
            SignalColor = "red";
            BaseLineColor = "fuchsia";

            MacdWidth = 1;
            SignalWidth = 1;
            BaseLineWidth = 1;
            
            Pane = 1;
        }
        
        // TODO: For profile-based creation
        // public MacdParams(ProfileSettings profileSettings) : base()
        // {
        //     
        // }
        
        public MovingAverageType MovingAverageType { get; set; }
        public int FastWeight { get; set; }
        public int SlowWeight { get; set; }
        public int SignalWeight { get; set; }
        
        public string MacdColor { get; set; }
        public int MacdWidth { get; set; } = 1;

        public string SignalColor { get; set; }
        public int SignalWidth { get; set; }
        
        public string BaseLineColor { get; set; }
        public int BaseLineWidth { get; set; }
    }
}
