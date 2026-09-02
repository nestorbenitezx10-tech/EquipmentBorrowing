using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<Borrowing?> GetByIdAsync(string id);
    Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(string studentId);
    Task SaveAsync(Borrowing borrowing);
    Task UpdateAsync(Borrowing borrowing);
}
