using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts.Studies
{
    [ExcludeFromCodeCoverage]
    public class RsiParams : StudyParams
    {
        public RsiParams()
        {
            OverboughtLevel = 70;
            OversoldLevel = 30;
            
            Pane = 2;
        }

        public RsiParams(IReadOnlyList<string> parameters)
        {
            OverboughtLevel = 70;
            OversoldLevel = 30;
            
            Pane = 2;
        }
        
        // TODO: For profile-based creation
        // public RelativeStrengthIndexParams(ProfileSettings profileSettings) : base()
        // {
        //     
        // }
        
        public int OverboughtLevel { get; set; } = 70;
        public int OversoldLevel { get; set; } = 30;
    }
}
