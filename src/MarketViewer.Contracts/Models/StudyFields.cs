using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class StudyFields
{
    public StudyType Type { get; set; }
    public string[] Parameters { get; set; }
}