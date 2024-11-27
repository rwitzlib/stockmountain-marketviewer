using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class AuthenticationRequest
{
    public string Token { get; set; }
}
