using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Mappings
{
    public class FilaAtendimentoMapping : IEntityTypeConfiguration<FilaAtendimento>
    {
        public void Configure(EntityTypeBuilder<FilaAtendimento> builder)
        {
            builder.ToTable(DataBaseConstants.TabelaFilaAtendimento.NomeTabela);

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).HasColumnName(DataBaseConstants.TabelaFilaAtendimento.Id);

            builder.Property(f => f.EventoId)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.EventoId)
                .IsRequired();

            builder.Property(f => f.NomeMulher)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.NomeMulher)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(f => f.Status)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(f => f.CriadoEm)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.CriadoEm)
                .IsRequired();

            builder.Property(f => f.ChamadoEm)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.ChamadoEm)
                .IsRequired(false);

            builder.Property(f => f.FinalizadoEm)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimento.FinalizadoEm)
                .IsRequired(false);

            builder.HasOne(f => f.Evento)
                .WithMany(e => e.Atendimentos)
                .HasForeignKey(f => f.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}