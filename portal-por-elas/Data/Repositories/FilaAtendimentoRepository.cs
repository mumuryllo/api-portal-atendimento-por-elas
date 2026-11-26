using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Domain.Entities;
using portal_por_elas.Domain.Enums;
using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Data.Repositories
{
    public class FilaAtendimentoRepository : Repository<FilaAtendimento>, IFilaAtendimentoRepository
    {
        public FilaAtendimentoRepository(PortalPorElasDbContext context) : base(context) { }

        public async Task<FilaAtendimento?> ObterPorIdComServicosAsync(Guid id)
        {
            return await _context.FilaAtendimentos
                .Include(f => f.Servicos)
                    .ThenInclude(s => s.Servico)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FilaAtendimento>> ObterPainelPublicoPorEventoAsync(Guid eventoId)
        {
            return await _context.FilaAtendimentos
                .Where(f => f.EventoId == eventoId && f.Status != StatusAtendimento.Finalizado)
                .Include(f => f.Servicos)
                    .ThenInclude(s => s.Servico)
                .OrderBy(f => f.CriadoEm)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<FilaAtendimento>> ObterTodoPainelPublicoAsync()
        {
            return await _context.FilaAtendimentos
                .Where(f => f.Status != StatusAtendimento.Finalizado)
                .Include(f => f.Evento)
                .Include(f => f.Servicos)
                    .ThenInclude(s => s.Servico)
                .OrderBy(f => f.CriadoEm)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> RemoverFinalizadosExpiradosAsync(DateTime limiteUtc)
        {
            var expirados = await _dbSet
                .Where(f => f.Status == StatusAtendimento.Finalizado
                         && f.FinalizadoEm.HasValue
                         && f.FinalizadoEm.Value <= limiteUtc)
                .ToListAsync();

            if (!expirados.Any())
                return 0;

            _dbSet.RemoveRange(expirados);
            return expirados.Count;
        }
    }
}