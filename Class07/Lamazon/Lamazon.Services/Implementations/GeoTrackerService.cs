using System.Net.Http.Json;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Models;

namespace Lamazon.Services.Implementations;

public class GeoTrackerService : IGeoTrackerService
{
    private readonly HttpClient _httpClient;

    public GeoTrackerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IpGeoInfo?> GetIpGeoInfoAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var url = $"json/{Uri.EscapeDataString(ipAddress)}?fields={IpGeoInfo.Fields}";
        var ipGeoInfo = await _httpClient.GetFromJsonAsync<IpGeoInfo>(url, cancellationToken);
        return ipGeoInfo;
    }

    public string GetCountryFlagUrl(string countryCode)
    {
        return $"https://flagcdn.com/w80/{countryCode.ToLower()}.png";
    }
}
