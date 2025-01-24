using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Api.Authorization;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public class RequiredPermissionsAttribute(UserRole[] permission) : Attribute
{
    public UserRole[] Permission { get; } = permission;
}
