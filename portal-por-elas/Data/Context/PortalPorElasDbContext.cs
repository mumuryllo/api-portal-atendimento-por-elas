using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Mappings;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Context
{
    public class PortalPorElasDbContext : DbContext

    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Evento> Eventos => Set<Evento>();
        public DbSet<Servico> Servicos => Set<Servico>();
        public DbSet<FilaAtendimento> FilaAtendimentos => Set<FilaAtendimento>();
        public DbSet<FilaAtendimentoServico> FilaAtendimentoServicos => Set<FilaAtendimentoServico>();

        public PortalPorElasDbContext(DbContextOptions<PortalPorElasDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            modelBuilder.ApplyConfiguration(new UserMapping());

            //Evento
            modelBuilder.ApplyConfiguration(new EventoMapping());

            //Servico
            modelBuilder.ApplyConfiguration(new ServicoMapping());

            //Fila Atendimento
            modelBuilder.ApplyConfiguration(new FilaAtendimentoMapping());  

            //Fila Atendimento Servico
            modelBuilder.ApplyConfiguration(new FilaAtendimentoServicoMapping());

            base.OnModelCreating(modelBuilder);
        }
    }
}
