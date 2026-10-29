using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IOrdersService
{
    Task<List<OrderLineItemViewModel>> GetCartLineItemsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);

    Task<string> CreateAsync(int userId, IReadOnlyCollection<int> productIds, string? ipAddress, CancellationToken cancellationToken);

    Task<OrderViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResultViewModel<OrderViewModel>> GetFilteredAsync(DataTableRequestViewModel request, CancellationToken cancellationToken = default);
    Task AcceptAsync(int id, CancellationToken cancellationToken = default);
    Task RejectAsync(int id, CancellationToken cancellationToken = default);
}
