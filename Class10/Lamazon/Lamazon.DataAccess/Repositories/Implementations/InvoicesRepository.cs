using System.Data.Common;
using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class InvoicesRepository : BaseRepository<Invoice>, IInvoicesRepository
{
    public InvoicesRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<int?> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        return await Table.MaxAsync(invoice => (int?)invoice.Id, cancellationToken) ?? 0;
    }
}
