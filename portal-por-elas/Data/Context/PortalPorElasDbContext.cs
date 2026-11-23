using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Mappings;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Context
{
    public class PortalPorElasDbContext : DbContext

    {
        public DbSet<User> Users => Set<User>();
        
        public PortalPorElasDbContext(DbContextOptions<PortalPorElasDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            modelBuilder.ApplyConfiguration(new UserMapping());

            base.OnModelCreating(modelBuilder);
        }
    }
}
