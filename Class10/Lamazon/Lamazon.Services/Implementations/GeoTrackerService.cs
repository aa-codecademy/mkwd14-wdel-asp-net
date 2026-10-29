using System.Net.Http.Json;
using System.Text.Json;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Models;
using Microsoft.Extensions.Logging;

namespace Lamazon.Services.Implementations;

public class GeoTrackerService : IGeoTrackerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoTrackerService> _logger;

    public GeoTrackerService(HttpClient httpClient, ILogger<GeoTrackerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IpGeoInfo?> GetIpGeoInfoAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"json/{Uri.EscapeDataString(ipAddress)}?fields={IpGeoInfo.Fields}";
            var ipGeoInfo = await _httpClient.GetFromJsonAsync<IpGeoInfo>(url, cancellationToken);
            return ipGeoInfo;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            // The location is nice to have, not required: the order is saved without it
            _logger.LogWarning(exception, "The geo-location API failed for IP {IpAddress}", ipAddress);
            return null;
        }
    }

    public string GetCountryFlagUrl(string countryCode)
    {
        return $"https://flagcdn.com/w80/{countryCode.ToLower()}.png";
    }
}
