using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Extensions;
using Lamazon.ViewModels.DataTable;
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

    public async Task<PagedResultViewModel<ProductCategoryViewModel>> GetFilteredAsync(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var pagedQueryModel = request.ToPagedQuery();
        var pagedResult = await _productCategoriesRepository.GetFilteredAsync(pagedQueryModel, cancellationToken);
        var result = _mapper.Map<PagedResultViewModel<ProductCategoryViewModel>>(pagedResult);
        return result;
    }

    public async Task CreateAsync(
        ProductCategoryViewModel productCategoryViewModel,
        CancellationToken cancellationToken = default)
    {
        var category = _mapper.Map<ProductCategory>(productCategoryViewModel);
        category.ProductCategoryStatusId = (int)ProductCategoryStatusEnum.Active;
        await _productCategoriesRepository.AddAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(
        ProductCategoryViewModel productCategoryViewModel,
        CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(productCategoryViewModel.Id, cancellationToken);
        _mapper.Map(productCategoryViewModel, category);
        await _productCategoriesRepository.UpdateAsync(category, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(id, cancellationToken);

        if (await _productCategoriesRepository.HasActiveProductsAsync(id, cancellationToken))
        {
            throw new BusinessRuleException($"The category \"{category.Name}\" still has products. Move or delete them first.");
        }

        category.ProductCategoryStatusId = (int)ProductCategoryStatusEnum.Deleted;
        await _productCategoriesRepository.UpdateAsync(category, cancellationToken);
    }

    private async Task<ProductCategory> FindCategoryAsync(int id, CancellationToken cancellationToken)
    {
        return await _productCategoriesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductCategory), id);
    }
}
