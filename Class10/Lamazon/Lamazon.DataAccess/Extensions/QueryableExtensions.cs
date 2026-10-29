using System.Linq.Expressions;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Extensions;

/// <summary>
/// Sorting and paging for the admin tables (DataTables sends the column name, the direction and the page).
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Sorts by the column the user clicked. Only the columns in <paramref name="sortableColumns"/> are allowed:
    /// the name comes from the browser, so we never trust it to point at any property it likes.
    /// An unknown or empty column name falls back to <paramref name="defaultSort"/>.
    /// </summary>
    public static IQueryable<T> SortBy<T>(
        this IQueryable<T> query,
        string? column,
        bool ascending,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortableColumns,
        Expression<Func<T, object>> defaultSort)
    {
        var keySelector = column is not null && sortableColumns.TryGetValue(column, out var selector)
            ? selector
            : defaultSort;

        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Counts the filtered rows, then loads one page of them.
    /// Call it on a query that is already filtered and sorted.
    /// </summary>
    public static async Task<PagedResultModel<T>> ToPagedResultAsync<T>(
        this IQueryable<T> filteredQuery,
        int totalRecords,
        int start,
        int length,
        CancellationToken cancellationToken = default)
    {
        var totalDisplayRecords = await filteredQuery.CountAsync(cancellationToken);

        var page = filteredQuery.Skip(Math.Max(start, 0));

        // DataTables sends length = -1 when the user picks "All"
        if (length > 0)
        {
            page = page.Take(length);
        }

        return new PagedResultModel<T>
        {
            Items = await page.ToListAsync(cancellationToken),
            TotalRecords = totalRecords,
            TotalDisplayRecords = totalDisplayRecords,
        };
    }
}
