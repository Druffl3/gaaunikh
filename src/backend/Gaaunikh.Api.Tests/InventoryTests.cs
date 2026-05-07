using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Gaaunikh.Api.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gaaunikh.Api.Tests;

public sealed class InventoryTests
{
    [Fact]
    public async Task StockSummary_ComputesOnHandReservedAndAvailableFromLedgerMovements()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var inventoryItemId = await SeedInventoryItemAsync(
            factory.Services,
            sku: "SPICE-SMOKED-PAPRIKA-200G",
            productSlug: "smoked-paprika",
            productName: "Smoked Paprika",
            weightLabel: "200g",
            unitPriceInr: 120m,
            reorderThreshold: 5);

        await SeedMovementAsync(factory.Services, inventoryItemId, 12, "StockAdjustment", "Restock");
        await SeedMovementAsync(factory.Services, inventoryItemId, -2, "StockAdjustment", "Damage");
        await SeedMovementAsync(factory.Services, inventoryItemId, 3, "Reservation", "PaidOrderReservation");

        var response = await client.GetAsync("/api/admin/inventory/summary");
        var payload = await response.Content.ReadFromJsonAsync<InventorySummaryResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var item = Assert.Single(payload!.Items);
        Assert.Equal("SPICE-SMOKED-PAPRIKA-200G", item.Sku);
        Assert.Equal(10, item.OnHand);
        Assert.Equal(3, item.Reserved);
        Assert.Equal(7, item.Available);
        Assert.False(item.IsLowStock);
    }

    [Fact]
    public async Task PaymentCapturedWebhook_ReservesPaidOrderInventoryExactlyOnce()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var inventoryItemId = await SeedInventoryItemAsync(
            factory.Services,
            sku: "BLEND-FIRE-MASALA-100G",
            productSlug: "fire-roast-masala",
            productName: "Fire Roast Masala",
            weightLabel: "100g",
            unitPriceInr: 140m,
            reorderThreshold: 2);

        await SeedMovementAsync(factory.Services, inventoryItemId, 8, "StockAdjustment", "Restock");

        var orderId = await SeedPendingOrderAsync(
            factory.Services,
            sku: "BLEND-FIRE-MASALA-100G",
            productSlug: "fire-roast-masala",
            productName: "Fire Roast Masala",
            weightLabel: "100g",
            unitPriceInr: 140m,
            quantity: 2);

        await SeedPaymentAttemptAsync(factory.Services, orderId, "order_inventory_paid");

        const string webhookPayload = """
            {
              "event": "payment.captured",
              "payload": {
                "payment": {
                  "entity": {
                    "id": "pay_inventory_paid",
                    "order_id": "order_inventory_paid",
                    "status": "captured"
                  }
                }
              }
            }
            """;
        var signature = ComputeSignature("test_webhook_secret", webhookPayload);

        using var firstRequest = CreateWebhookRequest(webhookPayload, signature);
        using var secondRequest = CreateWebhookRequest(webhookPayload, signature);

        var firstResponse = await client.SendAsync(firstRequest);
        var secondResponse = await client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var reservationMovements = await dbContext.InventoryMovements
            .Where(item =>
                item.InventoryItemId == inventoryItemId &&
                item.MovementType == "Reservation" &&
                item.Reason == "PaidOrderReservation")
            .ToListAsync();

        var response = await client.GetAsync("/api/admin/inventory/summary");
        var payload = await response.Content.ReadFromJsonAsync<InventorySummaryResponse>();
        var item = Assert.Single(payload!.Items);

        Assert.Single(reservationMovements);
        Assert.Equal(2, reservationMovements[0].QuantityDelta);
        Assert.Equal(8, item.OnHand);
        Assert.Equal(2, item.Reserved);
        Assert.Equal(6, item.Available);
    }

    [Fact]
    public async Task CreatingAndAdjustingInventory_MakesItemsAvailableToCatalog()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/admin/inventory/items", new
        {
            sku = "SPICE-KASHMIRI-GARLIC-150G",
            productSlug = "kashmiri-garlic-blend",
            productName = "Kashmiri Garlic Blend",
            category = "House Blend",
            shortDescription = "Garlic-forward finishing masala.",
            description = "A savory garlic and chili blend for fries, gravies, and finishing oils.",
            weightLabel = "150g",
            unitPriceInr = 165m,
            reorderThreshold = 4
        });

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var adjustmentResponse = await client.PostAsJsonAsync("/api/admin/inventory/adjustments", new
        {
            sku = "SPICE-KASHMIRI-GARLIC-150G",
            quantityDelta = 3,
            reason = "Restock",
            note = "Initial stock"
        });

        Assert.Equal(HttpStatusCode.OK, adjustmentResponse.StatusCode);

        var summaryResponse = await client.GetAsync("/api/admin/inventory/summary");
        var summaryPayload = await summaryResponse.Content.ReadFromJsonAsync<InventorySummaryResponse>();
        var item = Assert.Single(summaryPayload!.Items);

        Assert.Equal(3, item.OnHand);
        Assert.True(item.IsLowStock);

        var catalogResponse = await client.GetAsync("/api/catalog/products");
        var catalogPayload = await catalogResponse.Content.ReadFromJsonAsync<CatalogProductsResponse>();
        var product = Assert.Single(catalogPayload!.Products);

        Assert.Equal("kashmiri-garlic-blend", product.Slug);
        Assert.Equal("Kashmiri Garlic Blend", product.Name);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"inventory-tests-{Guid.NewGuid():N}";

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CommerceDatabase"] = "Host=localhost;Database=ignored;Username=ignored;Password=ignored",
                    ["Payments:Provider"] = "Razorpay",
                    ["Payments:Currency"] = "INR",
                    ["Payments:Razorpay:KeyId"] = "rzp_test_placeholder",
                    ["Payments:Razorpay:KeySecret"] = "test_key_secret",
                    ["Payments:Razorpay:WebhookSecret"] = "test_webhook_secret",
                    ["Payments:Razorpay:AutoCapture"] = "true"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CommerceDbContext>>();
                services.RemoveAll<CommerceDbContext>();
                services.RemoveAll<IRazorpayGateway>();
                services.AddDbContext<CommerceDbContext>(options => options.UseInMemoryDatabase(databaseName));
                services.AddSingleton<IRazorpayGateway, StubRazorpayGateway>();
            });
        });
    }

    private static async Task<Guid> SeedInventoryItemAsync(
        IServiceProvider services,
        string sku,
        string productSlug,
        string productName,
        string weightLabel,
        decimal unitPriceInr,
        int reorderThreshold)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            ProductSlug = productSlug,
            ProductName = productName,
            Category = "House Blend",
            ShortDescription = $"{productName} short description.",
            Description = $"{productName} detailed description.",
            WeightLabel = weightLabel,
            UnitPriceInr = unitPriceInr,
            ReorderThreshold = reorderThreshold,
            IsActive = true,
            CreatedUtc = DateTimeOffset.UtcNow,
            UpdatedUtc = DateTimeOffset.UtcNow
        };

        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync();
        return item.Id;
    }

    private static async Task SeedMovementAsync(
        IServiceProvider services,
        Guid inventoryItemId,
        int quantityDelta,
        string movementType,
        string reason)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        dbContext.InventoryMovements.Add(new InventoryMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            QuantityDelta = quantityDelta,
            MovementType = movementType,
            Reason = reason,
            CreatedUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedPendingOrderAsync(
        IServiceProvider services,
        string sku,
        string productSlug,
        string productName,
        string weightLabel,
        decimal unitPriceInr,
        int quantity)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Reference = "ORD-INVENTORY",
            Status = "PendingPayment",
            CustomerName = "Asha Raman",
            CustomerEmail = "asha@example.com",
            CustomerPhone = "+919999999999",
            ShippingAddressLine1 = "12 Spice Market Road",
            ShippingCity = "Bengaluru",
            ShippingState = "Karnataka",
            ShippingPostalCode = "560001",
            ShippingCountryCode = "IN",
            SubtotalInr = unitPriceInr * quantity,
            TotalInr = unitPriceInr * quantity,
            CreatedUtc = DateTimeOffset.UtcNow,
            Lines =
            [
                new OrderLine
                {
                    Id = Guid.NewGuid(),
                    Sku = sku,
                    ProductSlug = productSlug,
                    ProductName = productName,
                    WeightLabel = weightLabel,
                    UnitPriceInr = unitPriceInr,
                    Quantity = quantity,
                    LineTotalInr = unitPriceInr * quantity
                }
            ]
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order.Id;
    }

    private static async Task SeedPaymentAttemptAsync(IServiceProvider services, Guid orderId, string providerOrderId)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        dbContext.PaymentAttempts.Add(new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Provider = "Razorpay",
            Status = "Created",
            AmountInr = 280m,
            Currency = "INR",
            ProviderOrderId = providerOrderId,
            CreatedUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static HttpRequestMessage CreateWebhookRequest(string payload, string signature)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/razorpay/webhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Razorpay-Signature", signature);
        return request;
    }

    private static string ComputeSignature(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class StubRazorpayGateway : IRazorpayGateway
    {
        public Task<RazorpayOrderResult> CreateOrderAsync(RazorpayOrderRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RazorpayOrderResult(
                "order_test_123",
                "rzp_test_placeholder",
                request.AmountInr,
                request.Currency,
                request.AutoCapture));
        }
    }

    private sealed record InventorySummaryResponse(
        [property: JsonPropertyName("items")] IReadOnlyList<InventorySummaryItem> Items);

    private sealed record InventorySummaryItem(
        [property: JsonPropertyName("sku")] string Sku,
        [property: JsonPropertyName("onHand")] int OnHand,
        [property: JsonPropertyName("reserved")] int Reserved,
        [property: JsonPropertyName("available")] int Available,
        [property: JsonPropertyName("isLowStock")] bool IsLowStock);

    private sealed record CatalogProductsResponse(
        [property: JsonPropertyName("products")] IReadOnlyList<CatalogProductListItem> Products);

    private sealed record CatalogProductListItem(
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("name")] string Name);
}
