using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Mappings
{
    public class EventoMapping : IEntityTypeConfiguration<Evento>
    {
        public void Configure(EntityTypeBuilder<Evento> builder)
        {
            builder.ToTable(DataBaseConstants.TabelaEvento.NomeTabela);

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName(DataBaseConstants.TabelaEvento.Id);

            builder.Property(e => e.Nome)
                .HasColumnName(DataBaseConstants.TabelaEvento.Nome)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(e => e.DataEvento)
                .HasColumnName(DataBaseConstants.TabelaEvento.DataEvento)
                .IsRequired();

            builder.Property(e => e.HoraInicio)
                .HasColumnName(DataBaseConstants.TabelaEvento.HoraInicio)
                .IsRequired();

            builder.Property(e => e.HoraFim)
                .HasColumnName(DataBaseConstants.TabelaEvento.HoraFim)
                .IsRequired();

            builder.Property(e => e.Ativo)
                .HasColumnName(DataBaseConstants.TabelaEvento.Ativo)
                .IsRequired();

            builder.Property(e => e.CriadoEm)
                .HasColumnName(DataBaseConstants.TabelaEvento.CriadoEm)
                .IsRequired();
        }
    }
}