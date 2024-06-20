using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Study;

[ExcludeFromCodeCoverage]
public class StudyData
{
    public float Value { get; set; }
    public long Timestamp { get; set; }
}