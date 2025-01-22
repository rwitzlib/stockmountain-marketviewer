using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Api.Authorization
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method)]
    public class RequiredPermissionsAttribute(string[] permission) : Attribute
    {
        public string[] Permission { get; } = permission;
    }
}
