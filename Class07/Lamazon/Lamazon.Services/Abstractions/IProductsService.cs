using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IProductsService
{
    Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<ProductViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
