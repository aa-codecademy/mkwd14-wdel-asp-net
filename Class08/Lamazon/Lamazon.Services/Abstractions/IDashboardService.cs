using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}
