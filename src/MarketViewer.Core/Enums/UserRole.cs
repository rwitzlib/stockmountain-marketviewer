using System.Text.Json.Serialization;

namespace MarketViewer.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
public enum UserRole
{
    None,
    Basic,
    Advanced,
    Premium,
    Admin
}
