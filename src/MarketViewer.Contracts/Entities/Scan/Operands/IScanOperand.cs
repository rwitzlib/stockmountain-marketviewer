using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models.Scan.Operands;

public interface IScanOperand
{
    public OperandType GetOperandType();
    public int GetPriority();
    public bool HasTimeframe(out Timeframe timeframe);
}
