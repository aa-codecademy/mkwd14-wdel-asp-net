using System.Linq.Expressions;
using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Extensions;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class ProductsRepository : BaseRepository<Product>, IProductsRepository
{
    public ProductsRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    // The columns the admin table may sort by. The keys are the "name" of each DataTables column.
    private static readonly Dictionary<string, Expression<Func<Product, object>>> _sortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = product => product.Name,
        ["ProductCategory.Name"] = product => product.ProductCategory.Name,
        ["Price"] = product => product.Price,
    };

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

    public async Task<List<Product>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .AsNoTracking()
            .Where(product => product.IsFeatured)
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResultModel<Product>> GetFilteredAsync(
        int? categoryId,
        PagedQueryModel query,
        CancellationToken cancellationToken = default)
    {
        var products = ActiveProducts.AsNoTracking();
        var totalRecords = await products.CountAsync(cancellationToken);

        if (categoryId.HasValue)
        {
            products = products.Where(product => product.ProductCategoryId == categoryId.Value);
        }

        var searchValue = query.SearchValue;
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            products = products.Where(product => product.Name.Contains(searchValue));
        }

        return await products
            .SortBy(query.SortColumn, query.IsAscending, _sortableColumns, product => product.Id)
            .ToPagedResultAsync(totalRecords, query.Start, query.Length, cancellationToken);
    }
}
