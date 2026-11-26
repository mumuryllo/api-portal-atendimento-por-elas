using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IEventoRepository Eventos { get; }
        IServicoRepository Servicos { get; }
        IFilaAtendimentoRepository FilaAtendimentos { get; }

        Task<int> CommitAsync();
    }
}