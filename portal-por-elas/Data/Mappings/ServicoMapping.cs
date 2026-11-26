using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Mappings
{
    public class ServicoMapping : IEntityTypeConfiguration<Servico>
    {
        public void Configure(EntityTypeBuilder<Servico> builder)
        {
            builder.ToTable(DataBaseConstants.TabelaServico.NomeTabela);

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName(DataBaseConstants.TabelaServico.Id);

            builder.Property(s => s.Nome)
                .HasColumnName(DataBaseConstants.TabelaServico.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.Ativo)
                .HasColumnName(DataBaseConstants.TabelaServico.Ativo)
                .IsRequired();

            builder.HasData(
                new Servico(Guid.Parse("11111111-1111-1111-1111-111111111101"), "Manicure e Pedicure"),
                new Servico(Guid.Parse("11111111-1111-1111-1111-111111111102"), "Corte e Escova"),
                new Servico(Guid.Parse("11111111-1111-1111-1111-111111111103"), "Coloração e Hidratação"),
                new Servico(Guid.Parse("11111111-1111-1111-1111-111111111104"), "Limpeza de Pele"),
                new Servico(Guid.Parse("11111111-1111-1111-1111-111111111105"), "Design de Sobrancelhas")
            );
        }
    }
}