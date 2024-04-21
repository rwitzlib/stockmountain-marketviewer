using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class VolumeEntry
    {
        public long Timestamp { get; set; }
        public float Value { get; set; }
        public string Color { get; set; }
    }
}
