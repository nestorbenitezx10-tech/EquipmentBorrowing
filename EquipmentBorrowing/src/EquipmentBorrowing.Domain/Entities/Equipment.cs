namespace EquipmentBorrowing.Domain.Entities;

public class Equipment
{
    public string Id { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string IdentifierNormalized { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<Borrowing> Borrowings { get; } = new List<Borrowing>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string AssetTag
    {
        get => Identifier;
        set => Identifier = value;
    }
}
