using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Apex.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}