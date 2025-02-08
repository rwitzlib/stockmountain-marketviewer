using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models.Study;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(StudyConverter))]
public class StudyFields
{
    public StudyType Type { get; set; }
    public string[] Parameters { get; set; }
}