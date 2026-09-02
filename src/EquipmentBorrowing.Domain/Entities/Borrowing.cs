namespace EquipmentBorrowing.Domain.Entities;

public class Borrowing
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StudentId { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? BorrowedDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public BorrowingStatus Status { get; set; } = BorrowingStatus.Requested;
}
