using portal_por_elas.Interfaces.Repositories;

namespace portal_por_elas.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        Task<int> CommitAsync();


    }
}
