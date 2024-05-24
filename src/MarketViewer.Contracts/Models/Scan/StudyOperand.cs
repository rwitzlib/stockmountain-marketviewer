using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class StudyOperand
{
    public StudyType Type { get; set; }
    public string[] Parameters { get; set; }
}
