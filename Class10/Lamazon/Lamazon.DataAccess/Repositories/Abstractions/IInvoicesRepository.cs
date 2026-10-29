using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IInvoicesRepository : IRepository<Invoice>
{
    Task<int?> GetMaxIdAsync(CancellationToken cancellationToken = default);
}
