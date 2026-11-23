using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", table =>
        {
            table.HasCheckConstraint(
                "CK_Borrowings_ExpectedReturnDate",
                "\"ExpectedReturnDate\" > \"CheckoutDate\"");
            table.HasCheckConstraint(
                "CK_Borrowings_Status",
                "\"Status\" IN (0, 1, 2, 3, 4, 5)");
        });
        builder.HasKey(borrowing => borrowing.Id);
        builder.Property(borrowing => borrowing.Id).ValueGeneratedNever();
        builder.Property(borrowing => borrowing.StudentId).IsRequired();
        builder.Property(borrowing => borrowing.EquipmentId).IsRequired();
        builder.Property(borrowing => borrowing.CheckoutDate).IsRequired();
        builder.Property(borrowing => borrowing.ExpectedReturnDate).IsRequired();
        builder.Property(borrowing => borrowing.Status).IsRequired();
        builder.HasOne(borrowing => borrowing.Student)
            .WithMany(student => student.Borrowings)
            .HasForeignKey(borrowing => borrowing.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(borrowing => borrowing.Equipment)
            .WithMany(item => item.Borrowings)
            .HasForeignKey(borrowing => borrowing.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(borrowing => borrowing.StudentId)
            .HasDatabaseName("IX_Borrowings_StudentId");
        builder.HasIndex(borrowing => borrowing.EquipmentId)
            .HasFilter("\"Status\" = 0")
            .IsUnique()
            .HasDatabaseName("IX_Borrowings_EquipmentId_Active");
    }
}
