using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class ProductsRepository : BaseRepository<Product>, IProductsRepository
{
    public ProductsRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    private IQueryable<Product> ActiveProducts => Table
            .Where(product => product.ProductStatusId != (int)ProductStatusEnum.Deleted)
            .Include(product => product.ProductCategory);

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await ActiveProducts
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .AsNoTracking()
            .Where(product => product.IsFeatured)
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }
}
