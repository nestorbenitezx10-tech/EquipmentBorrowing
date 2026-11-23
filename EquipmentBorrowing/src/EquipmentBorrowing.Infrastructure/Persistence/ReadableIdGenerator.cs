using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

internal static class ReadableIdGenerator
{
    public static async Task<string> NextAsync(
        DbContext context,
        string sequenceName,
        string prefix,
        int digits,
        CancellationToken cancellationToken)
    {
        if (context.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
            return $"{prefix}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        if (!Regex.IsMatch(sequenceName, "^[A-Za-z0-9_]+$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Invalid sequence name.", nameof(sequenceName));

        var connection = context.Database.GetDbConnection();
        var closeAfterUse = connection.State != ConnectionState.Open;
        if (closeAfterUse)
            await context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT nextval('\"{sequenceName}\"')";
            var value = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            return $"{prefix}{value.ToString($"D{digits}", CultureInfo.InvariantCulture)}";
        }
        finally
        {
            if (closeAfterUse)
                await context.Database.CloseConnectionAsync();
        }
    }
}
