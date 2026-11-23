namespace EquipmentBorrowing.Domain.Entities;

public static class Normalizer
{
    public static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();
}
