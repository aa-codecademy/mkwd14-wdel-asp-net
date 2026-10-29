using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IProductsService
{
    Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<ProductViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResultViewModel<ProductViewModel>> GetFilteredAsync(ProductsDataTableRequestViewModel request, CancellationToken cancellationToken = default);
    Task CreateAsync(ProductViewModel productViewModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductViewModel productViewModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
