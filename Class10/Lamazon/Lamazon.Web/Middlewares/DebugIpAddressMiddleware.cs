using System.Net;

namespace Lamazon.Web.Middlewares;

public class DebugIpAddressMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Task<IPAddress?>? _publicIpAddress;

    public DebugIpAddressMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _publicIpAddress = GetPublicIpAddressAsync(httpClientFactory);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        if (remoteIpAddress is not null && IPAddress.IsLoopback(remoteIpAddress))
        {
            var publicIpAddress = await _publicIpAddress!;
            if (publicIpAddress is not null)
            {
                context.Connection.RemoteIpAddress = publicIpAddress;
            }
        }

        await _next(context);
    }

    private static async Task<IPAddress?> GetPublicIpAddressAsync(IHttpClientFactory httpClientFactory)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetStringAsync("https://api.ipify.org");
            _ = IPAddress.TryParse(response, out var publicIpAddress);
            return publicIpAddress;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
