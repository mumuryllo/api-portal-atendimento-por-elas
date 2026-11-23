using portal_por_elas.Data.Context;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PortalPorElasDbContext _context;

        public IUserRepository Users { get; }

        public UnitOfWork(
            PortalPorElasDbContext context,
            IUserRepository userRepository)
        {
            _context = context;
            Users = userRepository;
        }

        public async Task<int> CommitAsync()
            => await _context.SaveChangesAsync();
    }
}
