using MarketViewer.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.Models;

[ExcludeFromCodeCoverage]
public class Subject
{
    public string Email { get; set; }
    public UserRole Role { get; set; }
}