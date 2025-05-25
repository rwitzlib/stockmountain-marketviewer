using MarketViewer.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.Records;

[ExcludeFromCodeCoverage]
public class UserRecord
{
    public string Id { get; set; }
    public string AvatarUrl { get; set; }
    public float Credits { get; set; }
    public bool Public { get; set; }
    public UserRole Role { get; set; }
}
