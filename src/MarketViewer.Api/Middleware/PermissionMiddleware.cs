using MarketViewer.Api.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace MarketViewer.Api.Middleware;

[ExcludeFromCodeCoverage]
public class PermissionMiddleware(RequestDelegate next)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var endpoint = context.GetEndpoint();

            if (endpoint is null || !context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                await GenerateErrorResponse(context, "Permission denied: User does not have required permissions.");
                return;
            }

            var requiredPermissions = endpoint.Metadata
                .OfType<RequiredPermissionsAttribute>()
                .FirstOrDefault()?.Permission;

            var token = authorizationHeader.ToString().Split(" ").Last();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var propertiesClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "properties")?.Value;

            var subject = JsonSerializer.Deserialize<Subject>(propertiesClaim, _jsonSerializerOptions);

            if (requiredPermissions is null || !requiredPermissions.Contains(subject.Role))
            {
                await GenerateErrorResponse(context, "Permission denied: User does not have required permissions.");
                return;
            }

            context.Items.Add("UserId", subject.Email);

            await next(context);
        }
        catch (Exception ex)
        {
            await GenerateErrorResponse(context, "Permission denied: User does not have required permissions.");
            return;
        }
    }

    private static async Task GenerateErrorResponse(HttpContext context, string errorMessage)
    {
        var response = new { ErrorMessages = new string[] { errorMessage } };
        var json = JsonSerializer.Serialize(response);
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(json);
    }
}
