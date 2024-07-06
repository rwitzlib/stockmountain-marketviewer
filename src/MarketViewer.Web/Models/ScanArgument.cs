namespace MarketViewer.Web.Models
{
    public class ScanArgument
    {
        public string Id { get; set; }
        public string Operator { get; set; }
        public ScanArgument Argument { get; set; }
    }
}
