namespace Gaaunikh.Api.Data.Entities;

public sealed class InventoryItem
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int OnHandQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int ReorderThreshold { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public List<InventoryMovement> Movements { get; set; } = [];
}
