using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;
using portal_por_elas.Interfaces;
using System.Linq.Expressions;

namespace portal_por_elas.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly PortalPorElasDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(PortalPorElasDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.FindAsync(id);

        public async Task<List<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}