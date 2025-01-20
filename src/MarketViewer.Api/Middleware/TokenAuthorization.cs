using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace MarketViewer.Api.Middleware;

public class TokenAuthorization : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
    }
}
