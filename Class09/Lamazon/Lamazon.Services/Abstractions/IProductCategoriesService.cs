using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Abstractions;

public interface IProductCategoriesService
{
    Task<List<ProductCategoryViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductCategoryViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
