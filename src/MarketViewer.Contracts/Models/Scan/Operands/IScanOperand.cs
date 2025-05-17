using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;

namespace MarketViewer.Contracts.Models.Scan.Operands;

public interface IScanOperand
{
    public OperandType GetOperandType();
    public int GetPriority();
    public bool HasTimeframe(out Timeframe timeframe);
}
