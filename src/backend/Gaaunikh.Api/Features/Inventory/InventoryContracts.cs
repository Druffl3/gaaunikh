namespace Gaaunikh.Api.Features.Inventory;

public sealed record CreateInventoryItemRequest(
    string Sku,
    string ProductSlug,
    string ProductName,
    string Category,
    string ShortDescription,
    string Description,
    string WeightLabel,
    decimal UnitPriceInr,
    int ReorderThreshold);

public sealed record StockAdjustmentRequest(
    string Sku,
    int QuantityDelta,
    string Reason,
    string? Note);

public sealed record InventorySummaryResponse(IReadOnlyList<InventorySummaryItem> Items);

public sealed record InventorySummaryItem(
    string Sku,
    string ProductSlug,
    string ProductName,
    string Category,
    string WeightLabel,
    decimal UnitPriceInr,
    int OnHand,
    int Reserved,
    int Available,
    int ReorderThreshold,
    bool IsLowStock,
    IReadOnlyList<InventoryMovementEntry> RecentMovements);

public sealed record InventoryMovementEntry(
    int QuantityDelta,
    string MovementType,
    string Reason,
    string? Note,
    DateTimeOffset CreatedUtc);

public sealed record CatalogProductListItemDto(
    string Slug,
    string Name,
    string Category,
    string ShortDescription,
    decimal LowestPriceInr,
    decimal HighestPriceInr);

public sealed record CatalogProductDto(
    string Slug,
    string Name,
    string Category,
    string ShortDescription,
    string Description,
    IReadOnlyList<CatalogVariantDto> Variants);

public sealed record CatalogVariantDto(string WeightLabel, decimal PriceInr);

public sealed class InventoryValidationException : Exception
{
    public InventoryValidationException(string message)
        : base(message)
    {
    }
}
