using MarketViewer.Api.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace MarketViewer.Api.Middleware;

[ExcludeFromCodeCoverage]
public class PermissionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null || !context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Permission denied: User does not have required permissions.");
            return;
        }

        var requiredPermissions = endpoint.Metadata
            .OfType<RequiredPermissionsAttribute>()
            .FirstOrDefault()?.Permission;

        var token = authorizationHeader.ToString().Split(" ").Last();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var propertiesClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "properties")?.Value;

        if (propertiesClaim is not null)
        {
            var subject = JsonSerializer.Deserialize<Subject>(propertiesClaim);

            if (requiredPermissions is not null && !requiredPermissions.Contains(subject.Role))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Permission denied: User does not have required permissions.");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Permission denied: User does not have required permissions.");
            return;
        }

        await next(context);
    }
}
