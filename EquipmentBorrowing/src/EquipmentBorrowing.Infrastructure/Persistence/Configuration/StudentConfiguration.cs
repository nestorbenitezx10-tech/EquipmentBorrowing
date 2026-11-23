using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);
        builder.Property(student => student.Id).ValueGeneratedNever();
        builder.Property(student => student.StudentNumber).IsRequired().HasMaxLength(32);
        builder.Property(student => student.StudentNumberNormalized).IsRequired().HasMaxLength(32);
        builder.Property(student => student.FullName).IsRequired().HasMaxLength(120);
        builder.Property(student => student.Contact).HasMaxLength(254);
        builder.Property(student => student.IsEligible).IsRequired();
        builder.HasIndex(student => student.StudentNumberNormalized)
            .IsUnique()
            .HasDatabaseName("IX_Students_StudentNumberNormalized");
    }
}
