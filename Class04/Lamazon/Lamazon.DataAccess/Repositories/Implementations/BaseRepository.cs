using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected LamazonDbContext Context { get; }

    protected DbSet<T> Table => Context.Set<T>();

    protected BaseRepository(LamazonDbContext dbContext)
    {
        Context = dbContext;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Table.Add(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            Table.Update(entity);
        }
        await Context.SaveChangesAsync(cancellationToken);
    }
}
