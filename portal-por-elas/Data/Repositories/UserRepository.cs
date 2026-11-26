using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Data.Repositories;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Interfaces.Repositories;

namespace PortalOS.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(PortalPorElasDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}