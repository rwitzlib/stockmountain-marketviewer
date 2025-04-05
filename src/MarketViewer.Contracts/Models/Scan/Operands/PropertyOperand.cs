using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan.Operands;

[ExcludeFromCodeCoverage]
public class PropertyOperand : IScanOperand
{
    public string Property { get; set; }

    public OperandType GetOperandType()
    {
        return OperandType.Property;
    }

    public int GetPriority()
    {
        return 90;
    }

    public bool HasTimeframe(out Timeframe timeframe)
    {
        timeframe = null;
        return false;
    }
}
