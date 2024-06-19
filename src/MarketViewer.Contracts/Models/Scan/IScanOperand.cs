using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models.Scan;

public interface IScanOperand 
{
    public bool HasTimespan(out Timespan? timespan);
}
