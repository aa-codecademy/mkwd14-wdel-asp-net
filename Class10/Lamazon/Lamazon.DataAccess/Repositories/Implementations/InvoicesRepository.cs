using System.Linq.Expressions;
using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Extensions;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class InvoicesRepository : BaseRepository<Invoice>, IInvoicesRepository
{
    public InvoicesRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    private static readonly Dictionary<string, Expression<Func<Invoice, object>>> _sortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["InvoiceNumber"] = invoice => invoice.Id,
        ["User.FullName"] = invoice => invoice.User.FullName,
        ["TotalAmount"] = invoice => invoice.TotalAmount,
        ["InvoiceStatus"] = invoice => invoice.InvoiceStatusId,
    };

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(invoice => invoice.InvoiceLineItems)
            .Include(invoice => invoice.User)
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public async Task<PagedResultModel<Invoice>> GetFilteredAsync(
        PagedQueryModel query,
        CancellationToken cancellationToken = default)
    {
        var invoicesQuery = Table
            .AsNoTracking()
            .Include(invoice => invoice.User)
            .AsQueryable();

        var totalRecords = await invoicesQuery.CountAsync(cancellationToken);

        var searchValue = query.SearchValue;
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            invoicesQuery = invoicesQuery.Where(invoice =>
                invoice.InvoiceNumber.Contains(searchValue) ||
                invoice.User.FullName.Contains(searchValue));
        }

        return await invoicesQuery
            .SortBy(query.SortColumn, query.IsAscending, _sortableColumns, invoice => invoice.Id)
            .ToPagedResultAsync(totalRecords, query.Start, query.Length, cancellationToken);
    }

    public async Task<int?> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        return await Table.MaxAsync(invoice => (int?)invoice.Id, cancellationToken) ?? 0;
    }
}
