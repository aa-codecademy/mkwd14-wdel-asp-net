using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using MapsterMapper;

namespace Lamazon.Services.Implementations;

public class ProductCategoriesService : IProductCategoriesService
{
    private readonly IProductCategoriesRepository _productCategoriesRepository;
    private readonly IMapper _mapper;

    public ProductCategoriesService(
        IProductCategoriesRepository productCategoriesRepository,
        IMapper mapper
    )
    {
        _productCategoriesRepository = productCategoriesRepository;
        _mapper = mapper;
    }

    public async Task<List<ProductCategoryViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<ProductCategory> categories = await _productCategoriesRepository.GetAllAsync(cancellationToken);
        List<ProductCategoryViewModel> mappedCategories = _mapper.Map<List<ProductCategoryViewModel>>(categories);
        return mappedCategories;
    }

    public async Task<ProductCategoryViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ProductCategory? category = await _productCategoriesRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException(nameof(ProductCategory), id);
        }
        ProductCategoryViewModel productCategoryViewModel = _mapper.Map<ProductCategoryViewModel>(category);
        return productCategoryViewModel;
    }
}
