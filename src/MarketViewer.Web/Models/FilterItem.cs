using MarketViewer.Contracts.Models.ScanV2;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Models;

[ExcludeFromCodeCoverage]
public class FilterItem : IDisposable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CollectionModifier { get; set; }
    public OperandType FirstOperandType { get; set; }
    public IScanOperand FirstOperand { get; set; }
    public OperandType SecondOperandType { get; set; }
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; } = new();

    public FilterItem()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
