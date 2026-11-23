namespace ProyectoFinal.Models;

public sealed class ProductItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Alimentos";
    public int Quantity { get; set; } = 1;
    public DateTime ExpirationDate { get; set; } = DateTime.Today.AddDays(7);
    public bool IsConsumed { get; set; }
    public bool IsWasted { get; set; }
    public int DaysUntilExpiration => (ExpirationDate.Date - DateTime.Today).Days;
    public string Status => IsConsumed ? "Consumido a tiempo" : IsWasted ? "Desperdiciado" : DaysUntilExpiration < 0 ? "Vencido" : DaysUntilExpiration <= 3 ? "Próximo a vencer" : "Disponible";
}
