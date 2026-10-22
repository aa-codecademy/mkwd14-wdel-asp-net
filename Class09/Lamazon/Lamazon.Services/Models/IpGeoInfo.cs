namespace Lamazon.Services.Models;

public class IpGeoInfo
{
    public const string Fields = "status,message,countryCode";
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; } 
    public string? CountryCode { get; set; }
    public bool IsSuccess => Status.Equals("success", StringComparison.OrdinalIgnoreCase);
}
