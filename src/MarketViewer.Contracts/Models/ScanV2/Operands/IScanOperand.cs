using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models.ScanV2.Operands;

public interface IScanOperand
{
    public bool HasTimeframe(out int? multiplier, out Timespan? timespan);
}
