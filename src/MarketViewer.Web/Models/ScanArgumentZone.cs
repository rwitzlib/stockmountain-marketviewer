namespace MarketViewer.Web.Models
{
    public class ScanArgumentZone
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Depth { get; set; } = 0;
        public string Operator { get; set; }
        public ScanArgumentZone Argument { get; set; }
        public List<FilterItem> Filters { get; set; }
    }
}
