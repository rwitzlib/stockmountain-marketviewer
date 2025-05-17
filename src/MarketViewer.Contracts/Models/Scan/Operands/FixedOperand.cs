using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan.Operands;

[ExcludeFromCodeCoverage]
public class FixedOperand : IScanOperand
{

    public float Value { get; set; }

    public OperandType GetOperandType()
    {
        return OperandType.Fixed;
    }

    public int GetPriority()
    {
        return 100;
    }

    public bool HasTimeframe(out Timeframe timeframe)
    {
        timeframe = null;
        return false;
    }
}
