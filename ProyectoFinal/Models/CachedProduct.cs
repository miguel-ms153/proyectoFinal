using SQLite;

namespace ProyectoFinal.Models;

[Table("CachedProducts")]
public sealed class CachedProduct
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsConsumed { get; set; }
    public bool IsWasted { get; set; }

    public static CachedProduct From(ProductItem product) => new()
    {
        Id = product.Id.ToString(), Name = product.Name, Category = product.Category,
        Quantity = product.Quantity, ExpirationDate = product.ExpirationDate,
        IsConsumed = product.IsConsumed, IsWasted = product.IsWasted
    };

    public ProductItem ToProduct() => new()
    {
        Id = Guid.TryParse(Id, out var id) ? id : Guid.NewGuid(), Name = Name, Category = Category,
        Quantity = Quantity, ExpirationDate = ExpirationDate, IsConsumed = IsConsumed, IsWasted = IsWasted
    };
}
