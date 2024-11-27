namespace MarketViewer.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder RegisterMiddleware(this IApplicationBuilder builder)
    {
        return builder;
    }
}
