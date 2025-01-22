using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Api.Authorization;

[ExcludeFromCodeCoverage]
public class Subject
{
    public string Email { get; set; }
    public string Role { get; set; }
}