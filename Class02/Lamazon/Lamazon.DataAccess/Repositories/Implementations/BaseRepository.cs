using Lamazon.DataAccess.Repositories.Abstractions;

namespace Lamazon.DataAccess.Repositories.Implementations;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    public Task AddAsync(T entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
