using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class DbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();

        // This gives Entity Framework the explicit connection instruction it needs at design-time
        optionsBuilder.UseSqlite("Data Source=../../equipment_borrowing.db");

        return new EquipmentBorrowingDbContext(optionsBuilder.Options);
    }
}
