namespace EquipmentBorrowing.Domain.Entities;

public class Borrowing
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StudentId { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public DateOnly CheckoutDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly ExpectedReturnDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
    public DateOnly? ActualReturnDate { get; set; }
    public Student Student { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
    public BorrowingStatus Status { get; set; } = BorrowingStatus.Active;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime RequestDate
    {
        get => CheckoutDate.ToDateTime(TimeOnly.MinValue);
        set => CheckoutDate = DateOnly.FromDateTime(value);
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? BorrowedDate { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? ReturnDate
    {
        get => ActualReturnDate?.ToDateTime(TimeOnly.MinValue);
        set => ActualReturnDate = value is null ? null : DateOnly.FromDateTime(value.Value);
    }

    public void MarkReturned(DateOnly returnDate)
    {
        if (returnDate < CheckoutDate)
            throw new ArgumentException("Return date cannot precede checkout.", nameof(returnDate));
        Status = BorrowingStatus.Returned;
        ActualReturnDate = returnDate;
    }
}
