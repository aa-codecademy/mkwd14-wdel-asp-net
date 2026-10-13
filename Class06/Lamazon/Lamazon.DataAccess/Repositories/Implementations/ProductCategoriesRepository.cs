using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class ProductCategoriesRepository : BaseRepository<ProductCategory>, IProductCategoriesRepository
{
    public ProductCategoriesRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    private IQueryable<ProductCategory> ActiveCategories => Table
            .Where(category => category.ProductCategoryStatusId != (int)ProductCategoryStatusEnum.Deleted);

    public async Task<List<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<ProductCategory> categories = await ActiveCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
        return categories;
    }

    public async Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ActiveCategories
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }
}
