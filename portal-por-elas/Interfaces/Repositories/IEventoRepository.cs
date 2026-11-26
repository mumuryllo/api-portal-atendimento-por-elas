using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Interfaces.Repositories
{
    public interface IEventoRepository : IRepository<Evento>
    {
        Task<IEnumerable<Evento>> ObterEventosAtivosAsync();
        Task<IEnumerable<Evento>> ObterEventosComFilaAsync();
    }
}