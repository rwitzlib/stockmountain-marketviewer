using MarketViewer.Web.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts.Studies
{
    [ExcludeFromCodeCoverage]
    public class EmaParams : StudyParams
    {
        public EmaParams()
        {
            Weight = 1;
            Color = StudyColor.gray.ToString();
            Width = 1;
            Pane = 0;
        }

        public EmaParams(IReadOnlyList<string> parameters)
        {
            if (parameters.Count == 1 && int.TryParse(parameters[0], out var weight))
            {
                Weight = weight;
            }
            Color = StudyColor.gray.ToString();
            Width = 1;
            Pane = 0;
        }
        
        // TODO: For profile-based creation
        // public ExponentialMovingAverageParams(ProfileSettings profileSettings) : base()
        // {
        //     
        // }
        
        public int Weight { get; set; }
        public string Color { get; set; }
        public int Width { get; set; }
    }
}
