using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class Volume
    {
        public Volume()
        {
            VolumeSeries = new List<VolumeEntry>();
        }

        public string UpColor { get; set; } = "#26a69a";
        public string DownColor { get; set; } = "red";

        public List<VolumeEntry> VolumeSeries { get; set; }
    }
}
