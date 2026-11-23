using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Domain.Entities;
using System.Reflection;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    public EquipmentBorrowingDbContext(DbContextOptions<EquipmentBorrowingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically hooks up your 3 configuration maps
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Activity 4's PostgreSQL schema stores the public status labels as text.
        // Keep the retained SQLite model/migrations using their original integer enum representation.
        if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            modelBuilder.Entity<Borrowing>().Property(borrowing => borrowing.Status).HasConversion<string>();
            modelBuilder.Entity<Borrowing>().ToTable("Borrowings", table =>
                table.HasCheckConstraint("CK_Borrowings_Status", "\"Status\" IN ('Active', 'Returned', 'Requested', 'Approved', 'Borrowed', 'Overdue')"));
            modelBuilder.Entity<Borrowing>().HasIndex(borrowing => borrowing.EquipmentId)
                .HasFilter("\"Status\" = 'Active'");
        }
    }
}
