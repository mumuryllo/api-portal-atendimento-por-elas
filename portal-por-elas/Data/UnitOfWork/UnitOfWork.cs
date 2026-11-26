using portal_por_elas.Data.Context;
using portal_por_elas.Data.Repositories;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;
using PortalOS.Data.Repositories;

namespace portal_por_elas.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PortalPorElasDbContext _context;

        public IUserRepository Users { get; }
        public IEventoRepository Eventos { get; }
        public IServicoRepository Servicos { get; }
        public IFilaAtendimentoRepository FilaAtendimentos { get; }

        public UnitOfWork(PortalPorElasDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Eventos = new EventoRepository(_context);
            Servicos = new ServicoRepository(_context);
            FilaAtendimentos = new FilaAtendimentoRepository(_context);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}