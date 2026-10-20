using Lamazon.Services.Models;

namespace Lamazon.Services.Abstractions;

public interface IGeoTrackerService
{
    Task<IpGeoInfo?> GetIpGeoInfoAsync(string ipAddress, CancellationToken cancellationToken);
    string GetCountryFlagUrl(string countryCode);
}
