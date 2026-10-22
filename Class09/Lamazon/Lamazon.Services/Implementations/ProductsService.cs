using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
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
        Product? product = await _productsRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), id);
        }
        ProductViewModel productViewModel = _mapper.Map<ProductViewModel>(product);
        return productViewModel;
    }

    public async Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetFeaturedAsync(cancellationToken);
        List<ProductViewModel> mappedProducts = _mapper.Map<List<ProductViewModel>>(products);
        return mappedProducts;
    }
}
