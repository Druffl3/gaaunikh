namespace Gaaunikh.Api.Data.Entities;

public sealed class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public Guid? OrderId { get; set; }
    public int QuantityDelta { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
