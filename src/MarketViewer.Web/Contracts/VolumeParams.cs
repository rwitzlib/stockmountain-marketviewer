using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Contracts
{
    [ExcludeFromCodeCoverage]
    public class VolumeParams
    {
        public string ColorUp { get; set; } = "#26a69a";
        public string ColorDown { get; set; } = "#ff5252";
        public bool UseDifferentColors { get; set; } = true;
        public string UniformColor { get; set; } = "Blue";
    }
}
