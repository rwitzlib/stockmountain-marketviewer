using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts.Studies
{
    [ExcludeFromCodeCoverage]
    public class VwapParams : StudyParams
    {
        public VwapParams()
        {
            Color = "orange";
            Width = 1;
            Pane = 0;
        }

        public VwapParams(string[] parameters)
        {
            
        }

        // TODO: For profile-based creation
        // public VwapParams(ProfileSettings profileSettings)
        // {
        //     
        // }
        
        public string Color { get; set; }
        public int Width { get; set; }
    }
}
