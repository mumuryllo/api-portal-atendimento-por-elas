using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Interfaces.Repositories
{
    public interface IFilaAtendimentoRepository : IRepository<FilaAtendimento>
    {
        Task<FilaAtendimento?> ObterPorIdComServicosAsync(Guid id);
        Task<IEnumerable<FilaAtendimento>> ObterPainelPublicoPorEventoAsync(Guid eventoId);
        Task<IEnumerable<FilaAtendimento>> ObterTodoPainelPublicoAsync();
    }
}