using System.Linq.Expressions;
using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class ProductCategoriesRepository : BaseRepository<ProductCategory>, IProductCategoriesRepository
{
    public ProductCategoriesRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    private static readonly Dictionary<string, Expression<Func<ProductCategory, object>>> _sortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = category => category.Name,
    };

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

    public async Task<PagedResultModel<ProductCategory>> GetFilteredAsync(
        PagedQueryModel query,
        CancellationToken cancellationToken)
    {
        var categories = ActiveCategories.AsNoTracking();
        var totalRecords = await categories.CountAsync(cancellationToken);

        var searchValue = query.SearchValue;
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            categories = categories.Where(category => category.Name.Contains(searchValue));
        }

        // Sorting/Filtering
        var keySelector = query.SortColumn is not null && _sortableColumns.TryGetValue(query.SortColumn, out var selector) ? selector : category => category.Id;

        categories = query.IsAscending ? categories.OrderBy(keySelector) : categories.OrderByDescending(keySelector);

        // Paged Result Model
        var totalDisplayRecords = await categories.CountAsync(cancellationToken);

        var page = categories.Skip(Math.Max(query.Start, 0));

        if (query.Length > 0)
        {
            page = page.Take(query.Length);
        }

        return new PagedResultModel<ProductCategory>
        {
            Items = await page.ToListAsync(cancellationToken),
            TotalRecords = totalRecords,
            TotalDisplayRecords = totalDisplayRecords
        };
    }

    public Task<bool> HasActiveProductsAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
