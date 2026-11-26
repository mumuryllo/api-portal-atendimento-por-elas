using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Interfaces.Repositories
{
    public interface IServicoRepository : IRepository<Servico>
    {
        Task<IEnumerable<Servico>> ObterTodosAtivosAsync();
        Task<IEnumerable<Servico>> ObterPorIdsAsync(IEnumerable<Guid> ids);
    }
}