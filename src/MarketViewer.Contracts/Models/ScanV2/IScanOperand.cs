using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models.ScanV2;

public interface IScanOperand
{
    public bool HasTimespan(out Timespan? timespan);
}
