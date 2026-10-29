using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Constants;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    public UsersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Table
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await Table
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await Table.AnyAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<List<User>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
    {
        return await Table
            .AsNoTracking()
            .Where(user => user.RoleKey == Roles.Admin)
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);
    }
}
