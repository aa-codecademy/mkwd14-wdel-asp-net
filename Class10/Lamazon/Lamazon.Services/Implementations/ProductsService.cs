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

public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IMapper _mapper;

    public ProductsService(
        IProductsRepository productsRepository,
        IMapper mapper
    )
    {
        _productsRepository = productsRepository;
        _mapper = mapper;
    }

    public async Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetAllAsync(cancellationToken);
        List<ProductViewModel> mappedProducts = _mapper.Map<List<ProductViewModel>>(products);
        return mappedProducts;
    }

    public async Task<ProductViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Product product = await FindProductAsync(id, cancellationToken);
        ProductViewModel productViewModel = _mapper.Map<ProductViewModel>(product);
        return productViewModel;
    }

    public async Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetFeaturedAsync(cancellationToken);
        List<ProductViewModel> mappedProducts = _mapper.Map<List<ProductViewModel>>(products);
        return mappedProducts;
    }

    public async Task<PagedResultViewModel<ProductViewModel>> GetFilteredAsync(
        ProductsDataTableRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var pagedQueryModel = request.ToPagedQuery();
        var pagedResult = await _productsRepository.GetFilteredAsync(request.CategoryId, pagedQueryModel, cancellationToken);
        var result = _mapper.Map<PagedResultViewModel<ProductViewModel>>(pagedResult);
        return result;
    }

    public async Task CreateAsync(
        ProductViewModel productViewModel,
        CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(productViewModel);
        product.ProductStatusId = (int)ProductStatusEnum.Active;
        await _productsRepository.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(
        ProductViewModel productViewModel,
        CancellationToken cancellationToken = default)
    {
        // The product is tracked (GetByIdAsync has no AsNoTracking): only the columns that changed are updated
        var product = await FindProductAsync(productViewModel.Id, cancellationToken);
        _mapper.Map(productViewModel, product);
        await _productsRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Soft delete: old orders still point to this product, so the row stays
        var product = await FindProductAsync(id, cancellationToken);
        product.ProductStatusId = (int)ProductStatusEnum.Deleted;
        await _productsRepository.UpdateAsync(product, cancellationToken);
    }

    private async Task<Product> FindProductAsync(int id, CancellationToken cancellationToken)
    {
        return await _productsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
    }
}
