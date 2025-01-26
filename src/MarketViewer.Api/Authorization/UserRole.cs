using System.Text.Json.Serialization;

namespace MarketViewer.Api.Authorization;

[JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
public enum UserRole
{
    None,
    Starter,
    Advanced,
    Premium,
    Admin
}
