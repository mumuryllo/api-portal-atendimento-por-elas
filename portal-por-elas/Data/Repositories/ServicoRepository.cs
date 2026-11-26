using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Data.Repositories
{
    public class ServicoRepository : Repository<Servico>, IServicoRepository
    {
        public ServicoRepository(PortalPorElasDbContext context) : base(context) { }

        public async Task<IEnumerable<Servico>> ObterTodosAtivosAsync()
        {
            return await _context.Servicos
                .Where(s => s.Ativo)
                .OrderBy(s => s.Nome)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Servico>> ObterPorIdsAsync(IEnumerable<Guid> ids)
        {
            var listaIds = ids.Distinct().ToList();
            return await _context.Servicos
                .Where(s => s.Ativo && listaIds.Contains(s.Id))
                .ToListAsync();
        }
    }
}