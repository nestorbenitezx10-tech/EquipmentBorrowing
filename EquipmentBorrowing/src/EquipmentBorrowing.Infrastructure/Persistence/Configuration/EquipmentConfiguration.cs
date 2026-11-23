using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Identifier).IsRequired().HasMaxLength(32);
        builder.Property(item => item.IdentifierNormalized).IsRequired().HasMaxLength(32);
        builder.Property(item => item.Name).IsRequired().HasMaxLength(120);
        builder.Property(item => item.Description).HasMaxLength(500);
        builder.Property(item => item.IsAvailable).IsRequired();
        builder.HasIndex(item => item.IdentifierNormalized)
            .IsUnique()
            .HasDatabaseName("IX_Equipment_IdentifierNormalized");
    }
}
