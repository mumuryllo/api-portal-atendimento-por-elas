using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Data.Repositories
{
    public class EventoRepository : Repository<Evento>, IEventoRepository
    {
        public EventoRepository(PortalPorElasDbContext context) : base(context) { }

        public async Task<IEnumerable<Evento>> ObterEventosAtivosAsync()
        {
            return await _context.Eventos
                .Where(e => e.Ativo)
                .OrderByDescending(e => e.DataEvento)
                .ThenBy(e => e.HoraInicio)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Evento>> ObterEventosComFilaAsync()
        {
            return await _context.Eventos
                .Where(e => e.Ativo)
                .Include(e => e.Atendimentos)
                    .ThenInclude(a => a.Servicos)
                        .ThenInclude(s => s.Servico)
                .OrderByDescending(e => e.DataEvento)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}