using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IOrdersService
{
    Task<List<OrderLineItemViewModel>> GetCartLineItemsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);

    Task<string> CreateAsync(int userId, IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
