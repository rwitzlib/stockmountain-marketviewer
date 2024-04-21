using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts.Studies
{
    [ExcludeFromCodeCoverage]
    public class SmaParams : StudyParams
    {
        public SmaParams()
        {
            Weight = 1;
            Color = "gray";
            Width = 1;
            Pane = 0;
        }

        public SmaParams(IReadOnlyList<string> parameters)
        {
            if (parameters.Count == 1 && int.TryParse(parameters[0], out var weight))
            {
                Weight = weight;
            }
            Color = "gray";
            Width = 1;
            Pane = 0;
        }

        // TODO: For profile-based creation
        // public SimpleMovingAverageParams(ProfileSettings profileSettings) : base()
        // {
        //     
        // }
        
        public int Weight { get; set; }
        public string Color { get; set; }
        public int Width { get; set; }
    }
}
