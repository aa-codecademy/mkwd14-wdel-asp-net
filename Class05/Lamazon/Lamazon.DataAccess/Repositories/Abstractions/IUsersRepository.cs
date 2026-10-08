using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IUsersRepository : IRepository<User>
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
}
