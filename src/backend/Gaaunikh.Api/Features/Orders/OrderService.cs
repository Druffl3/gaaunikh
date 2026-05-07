using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gaaunikh.Api.Features.Orders;

public sealed class OrderService
{
    private readonly CommerceDbContext _dbContext;

    public OrderService(CommerceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckoutResponse> CreateAsync(CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            throw new CheckoutValidationException("Checkout requires at least one cart line.");
        }

        var orderLines = new List<OrderLine>();
        decimal subtotalInr = 0;

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new CheckoutValidationException("Checkout line quantity must be at least 1.");
            }

            var normalizedProductSlug = line.ProductSlug.Trim().ToLowerInvariant();
            var normalizedWeightLabel = line.WeightLabel.Trim();
            var inventoryItem = await _dbContext.InventoryItems.SingleOrDefaultAsync(
                item =>
                    item.IsActive &&
                    item.ProductSlug == normalizedProductSlug &&
                    item.WeightLabel == normalizedWeightLabel,
                cancellationToken);

            if (inventoryItem is null)
            {
                throw new CheckoutValidationException($"Unknown product '{line.ProductSlug}'.");
            }

            var lineTotalInr = inventoryItem.UnitPriceInr * line.Quantity;
            subtotalInr += lineTotalInr;

            orderLines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                Sku = inventoryItem.Sku,
                ProductSlug = inventoryItem.ProductSlug,
                ProductName = inventoryItem.ProductName,
                WeightLabel = inventoryItem.WeightLabel,
                UnitPriceInr = inventoryItem.UnitPriceInr,
                Quantity = line.Quantity,
                LineTotalInr = lineTotalInr
            });
        }

        var timestamp = DateTimeOffset.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Reference = $"ORD-{Guid.NewGuid():N}"[..14].ToUpperInvariant(),
            Status = "PendingPayment",
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            ShippingAddressLine1 = request.ShippingAddress.Line1.Trim(),
            ShippingAddressLine2 = request.ShippingAddress.Line2?.Trim(),
            ShippingCity = request.ShippingAddress.City.Trim(),
            ShippingState = request.ShippingAddress.State.Trim(),
            ShippingPostalCode = request.ShippingAddress.PostalCode.Trim(),
            ShippingCountryCode = request.ShippingAddress.CountryCode.Trim().ToUpperInvariant(),
            SubtotalInr = subtotalInr,
            TotalInr = subtotalInr,
            CreatedUtc = timestamp,
            Lines = orderLines
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CheckoutResponse(order.Id, order.Reference, order.Status, order.SubtotalInr, order.TotalInr);
    }
}

public sealed class CheckoutValidationException : Exception
{
    public CheckoutValidationException(string message)
        : base(message)
    {
    }
}
