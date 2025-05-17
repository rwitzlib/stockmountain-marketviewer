using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Study;

[ExcludeFromCodeCoverage]
public class StudyResponse
{
    public string Name { get; set; }
    public string[] Parameters { get; set; }
    public List<List<LineEntry>> Results { get; set; }
}