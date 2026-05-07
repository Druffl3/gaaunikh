namespace Gaaunikh.Api.Data.Entities;

public sealed class InventoryItem
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WeightLabel { get; set; } = string.Empty;
    public decimal UnitPriceInr { get; set; }
    public int OnHandQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int ReorderThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public List<InventoryMovement> Movements { get; set; } = [];
}
