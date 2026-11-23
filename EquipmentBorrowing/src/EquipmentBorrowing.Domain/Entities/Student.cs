namespace EquipmentBorrowing.Domain.Entities;

public class Student
{
    public string Id { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentNumberNormalized { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public bool IsEligible { get; set; } = true;
    public ICollection<Borrowing> Borrowings { get; } = new List<Borrowing>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Name
    {
        get => FullName;
        set => FullName = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Email
    {
        get => Contact ?? string.Empty;
        set => Contact = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsActive
    {
        get => IsEligible;
        set => IsEligible = value;
    }
}
