using SQLite;

namespace ProyectoFinal.Models;

[Table("PendingSyncOperations")]
public sealed class PendingSyncOperation
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
