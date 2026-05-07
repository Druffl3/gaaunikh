using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gaaunikh.Api.Features.Inventory;

public sealed class InventoryService
{
    private readonly CommerceDbContext _dbContext;

    public InventoryService(CommerceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InventorySummaryItem>> GetSummaryAsync(
        string? search,
        string? category,
        string? sku,
        CancellationToken cancellationToken)
    {
        var items = await QueryInventoryItems(search, category, sku)
            .OrderBy(item => item.ProductName)
            .ThenBy(item => item.WeightLabel)
            .ToListAsync(cancellationToken);

        return items.Select(MapSummary).ToArray();
    }

    public async Task<InventorySummaryItem> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var normalizedSku = NormalizeRequired(request.Sku, "SKU");
        var normalizedSlug = NormalizeRequired(request.ProductSlug, "product slug").ToLowerInvariant();
        var normalizedWeightLabel = NormalizeRequired(request.WeightLabel, "weight label");

        if (request.UnitPriceInr <= 0)
        {
            throw new InventoryValidationException("Unit price must be greater than zero.");
        }

        if (request.ReorderThreshold < 0)
        {
            throw new InventoryValidationException("Reorder threshold cannot be negative.");
        }

        var skuExists = await _dbContext.InventoryItems.AnyAsync(item => item.Sku == normalizedSku, cancellationToken);
        if (skuExists)
        {
            throw new InventoryValidationException($"SKU '{normalizedSku}' already exists.");
        }

        var variantExists = await _dbContext.InventoryItems.AnyAsync(
            item => item.ProductSlug == normalizedSlug && item.WeightLabel == normalizedWeightLabel,
            cancellationToken);
        if (variantExists)
        {
            throw new InventoryValidationException(
                $"Variant '{normalizedWeightLabel}' already exists for product '{normalizedSlug}'.");
        }

        var timestamp = DateTimeOffset.UtcNow;
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Sku = normalizedSku,
            ProductSlug = normalizedSlug,
            ProductName = NormalizeRequired(request.ProductName, "product name"),
            Category = NormalizeRequired(request.Category, "category"),
            ShortDescription = NormalizeRequired(request.ShortDescription, "short description"),
            Description = NormalizeRequired(request.Description, "description"),
            WeightLabel = normalizedWeightLabel,
            UnitPriceInr = request.UnitPriceInr,
            ReorderThreshold = request.ReorderThreshold,
            IsActive = true,
            CreatedUtc = timestamp,
            UpdatedUtc = timestamp
        };

        _dbContext.InventoryItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetSummaryBySkuAsync(normalizedSku, cancellationToken);
    }

    public async Task<InventorySummaryItem> AdjustStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        if (request.QuantityDelta == 0)
        {
            throw new InventoryValidationException("Stock adjustment quantity must not be zero.");
        }

        var normalizedSku = NormalizeRequired(request.Sku, "SKU");
        var item = await _dbContext.InventoryItems
            .SingleOrDefaultAsync(inventoryItem => inventoryItem.Sku == normalizedSku, cancellationToken);

        if (item is null)
        {
            throw new InventoryValidationException($"Unknown SKU '{normalizedSku}'.");
        }

        _dbContext.InventoryMovements.Add(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = item.Id,
            QuantityDelta = request.QuantityDelta,
            MovementType = InventoryReservationPolicy.StockAdjustmentMovementType,
            Reason = NormalizeRequired(request.Reason, "reason"),
            Note = request.Note?.Trim(),
            CreatedUtc = DateTimeOffset.UtcNow
        });
        item.UpdatedUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetSummaryBySkuAsync(normalizedSku, cancellationToken);
    }

    public async Task ReservePaidOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null || !InventoryReservationPolicy.ShouldReserve(order.Status))
        {
            return;
        }

        var skuSet = order.Lines
            .Where(line => !string.IsNullOrWhiteSpace(line.Sku))
            .Select(line => line.Sku)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var inventoryItemsBySku = await _dbContext.InventoryItems
            .Where(item => skuSet.Contains(item.Sku))
            .ToDictionaryAsync(item => item.Sku, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var line in order.Lines)
        {
            var inventoryItem = ResolveInventoryItem(inventoryItemsBySku, line);
            if (inventoryItem is null)
            {
                continue;
            }

            var existingReservation = await _dbContext.InventoryMovements
                .Where(movement =>
                    movement.InventoryItemId == inventoryItem.Id &&
                    movement.OrderId == order.Id &&
                    movement.MovementType == InventoryReservationPolicy.ReservationMovementType &&
                    movement.Reason == InventoryReservationPolicy.PaidOrderReservationReason)
                .SumAsync(movement => movement.QuantityDelta, cancellationToken);

            var missingReservation = line.Quantity - existingReservation;
            if (missingReservation <= 0)
            {
                continue;
            }

            _dbContext.InventoryMovements.Add(new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryItemId = inventoryItem.Id,
                OrderId = order.Id,
                QuantityDelta = missingReservation,
                MovementType = InventoryReservationPolicy.ReservationMovementType,
                Reason = InventoryReservationPolicy.PaidOrderReservationReason,
                Note = $"Reserved for order {order.Reference}.",
                CreatedUtc = DateTimeOffset.UtcNow
            });
            inventoryItem.UpdatedUtc = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogProductListItemDto>> GetCatalogProductsAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken)
    {
        var inventoryItems = await QueryInventoryItems(search, category, null)
            .Where(item => item.IsActive)
            .ToListAsync(cancellationToken);

        return inventoryItems
            .GroupBy(item => new { item.ProductSlug, item.ProductName, item.Category, item.ShortDescription })
            .OrderBy(group => group.Key.ProductName)
            .Select(group => new CatalogProductListItemDto(
                group.Key.ProductSlug,
                group.Key.ProductName,
                group.Key.Category,
                group.Key.ShortDescription,
                group.Min(item => item.UnitPriceInr),
                group.Max(item => item.UnitPriceInr)))
            .ToArray();
    }

    public async Task<CatalogProductDto?> GetCatalogProductAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        var inventoryItems = await _dbContext.InventoryItems
            .Where(item => item.IsActive && item.ProductSlug == normalizedSlug)
            .OrderBy(item => item.UnitPriceInr)
            .ToListAsync(cancellationToken);

        if (inventoryItems.Count == 0)
        {
            return null;
        }

        var firstItem = inventoryItems[0];
        return new CatalogProductDto(
            firstItem.ProductSlug,
            firstItem.ProductName,
            firstItem.Category,
            firstItem.ShortDescription,
            firstItem.Description,
            inventoryItems
                .Select(item => new CatalogVariantDto(item.WeightLabel, item.UnitPriceInr))
                .ToArray());
    }

    private IQueryable<InventoryItem> QueryInventoryItems(string? search, string? category, string? sku)
    {
        var query = _dbContext.InventoryItems.Include(item => item.Movements).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(item =>
                item.ProductName.ToLower().Contains(normalizedSearch) ||
                item.ProductSlug.ToLower().Contains(normalizedSearch) ||
                item.Sku.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLowerInvariant();
            query = query.Where(item => item.Category.ToLower() == normalizedCategory);
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var normalizedSku = sku.Trim();
            query = query.Where(item => item.Sku == normalizedSku);
        }

        return query.Where(item =>
            !string.IsNullOrWhiteSpace(item.ProductSlug) &&
            !string.IsNullOrWhiteSpace(item.ProductName) &&
            !string.IsNullOrWhiteSpace(item.WeightLabel));
    }

    private async Task<InventorySummaryItem> GetSummaryBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        var item = await _dbContext.InventoryItems
            .Include(inventoryItem => inventoryItem.Movements)
            .SingleAsync(inventoryItem => inventoryItem.Sku == sku, cancellationToken);

        return MapSummary(item);
    }

    private static InventoryItem? ResolveInventoryItem(
        IReadOnlyDictionary<string, InventoryItem> inventoryItemsBySku,
        OrderLine line)
    {
        if (!string.IsNullOrWhiteSpace(line.Sku) &&
            inventoryItemsBySku.TryGetValue(line.Sku, out var inventoryItem))
        {
            return inventoryItem;
        }

        return inventoryItemsBySku.Values.FirstOrDefault(item =>
            item.ProductSlug.Equals(line.ProductSlug, StringComparison.OrdinalIgnoreCase) &&
            item.WeightLabel.Equals(line.WeightLabel, StringComparison.OrdinalIgnoreCase));
    }

    private static InventorySummaryItem MapSummary(InventoryItem item)
    {
        var onHand = item.Movements
            .Where(movement => movement.MovementType == InventoryReservationPolicy.StockAdjustmentMovementType)
            .Sum(movement => movement.QuantityDelta);
        var reserved = item.Movements
            .Where(movement => movement.MovementType == InventoryReservationPolicy.ReservationMovementType)
            .Sum(movement => movement.QuantityDelta);
        var available = onHand - reserved;

        return new InventorySummaryItem(
            item.Sku,
            item.ProductSlug,
            item.ProductName,
            item.Category,
            item.WeightLabel,
            item.UnitPriceInr,
            onHand,
            reserved,
            available,
            item.ReorderThreshold,
            available <= item.ReorderThreshold,
            item.Movements
                .OrderByDescending(movement => movement.CreatedUtc)
                .Take(5)
                .Select(movement => new InventoryMovementEntry(
                    movement.QuantityDelta,
                    movement.MovementType,
                    movement.Reason,
                    movement.Note,
                    movement.CreatedUtc))
                .ToArray());
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InventoryValidationException($"Inventory {fieldName} is required.");
        }

        return value.Trim();
    }
}
