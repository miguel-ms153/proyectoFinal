namespace EcoScanHogar.Api.Models;

public sealed class ProductEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Alimentos";
    public int Quantity { get; set; } = 1;
    public DateTime ExpirationDate { get; set; }
    public bool IsConsumed { get; set; }
    public bool IsWasted { get; set; }
}
