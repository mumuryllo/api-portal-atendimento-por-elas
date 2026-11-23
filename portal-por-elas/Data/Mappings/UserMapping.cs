using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_por_elas.Domain.Constants;
using portal_por_elas.Domain.Entities;

namespace portal_por_elas.Data.Mappings
{
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(DataBaseConstants.Tables.Users);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName(DataBaseConstants.Columns.Id);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName(DataBaseConstants.Columns.Email)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .HasColumnName(DataBaseConstants.Columns.PasswordHash)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasColumnName(DataBaseConstants.Columns.Role)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Active)
                .HasColumnName(DataBaseConstants.Columns.Active)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName(DataBaseConstants.Columns.CreatedAt)
                .IsRequired();

        }
    }
}
