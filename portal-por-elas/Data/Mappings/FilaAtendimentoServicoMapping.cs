using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Mappings
{
    public class FilaAtendimentoServicoMapping : IEntityTypeConfiguration<FilaAtendimentoServico>
    {
        public void Configure(EntityTypeBuilder<FilaAtendimentoServico> builder)
        {
            builder.ToTable(DataBaseConstants.TabelaFilaAtendimentoServico.NomeTabela);

            builder.HasKey(fs => new { fs.FilaAtendimentoId, fs.ServicoId });

            builder.Property(fs => fs.FilaAtendimentoId)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimentoServico.FilaAtendimentoId);

            builder.Property(fs => fs.ServicoId)
                .HasColumnName(DataBaseConstants.TabelaFilaAtendimentoServico.ServicoId);

            builder.HasOne(fs => fs.FilaAtendimento)
                .WithMany(f => f.Servicos)
                .HasForeignKey(fs => fs.FilaAtendimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fs => fs.Servico)
                .WithMany(s => s.AtendimentoServicos)
                .HasForeignKey(fs => fs.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}