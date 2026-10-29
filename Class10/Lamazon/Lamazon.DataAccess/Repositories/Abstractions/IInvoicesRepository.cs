using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IInvoicesRepository : IRepository<Invoice>
{
    Task<int?> GetMaxIdAsync(CancellationToken cancellationToken = default);

    Task<PagedResultModel<Invoice>> GetFilteredAsync(PagedQueryModel query, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
