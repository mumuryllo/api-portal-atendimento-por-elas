using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
