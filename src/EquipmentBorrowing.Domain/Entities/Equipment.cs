namespace EquipmentBorrowing.Domain.Entities;

public class Equipment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
