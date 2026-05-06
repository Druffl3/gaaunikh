using Gaaunikh.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gaaunikh.Api.Tests;

public sealed class CheckoutApiTests
{
    [Fact]
    public async Task Checkout_PersistsOrderWithImmutableLines()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders/checkout", new
        {
            customerName = "Asha Raman",
            customerEmail = "asha@example.com",
            customerPhone = "+919999999999",
            shippingAddress = new
            {
                line1 = "12 Spice Market Road",
                line2 = "Floor 2",
                city = "Bengaluru",
                state = "Karnataka",
                postalCode = "560001",
                countryCode = "IN"
            },
            lines = new[]
            {
                new
                {
                    productSlug = "kashmiri-chili-powder",
                    weightLabel = "100g",
                    unitPriceInr = 1m,
                    quantity = 2
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CheckoutApiResponse>();
        Assert.NotNull(payload);
        Assert.Equal("PendingPayment", payload!.Status);
        Assert.Equal(190m, payload.SubtotalInr);
        Assert.Equal(190m, payload.TotalInr);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var order = await dbContext.Orders.Include(item => item.Lines).SingleAsync();
        var orderLine = Assert.Single(order.Lines);

        Assert.Equal("Asha Raman", order.CustomerName);
        Assert.Equal("asha@example.com", order.CustomerEmail);
        Assert.Equal("PendingPayment", order.Status);
        Assert.Equal(190m, order.TotalInr);
        Assert.Equal("Kashmiri Chili Powder", orderLine.ProductName);
        Assert.Equal(95m, orderLine.UnitPriceInr);
        Assert.Equal(2, orderLine.Quantity);
        Assert.Equal(190m, orderLine.LineTotalInr);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"checkout-tests-{Guid.NewGuid():N}";

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CommerceDbContext>>();
                services.RemoveAll<CommerceDbContext>();
                services.AddDbContext<CommerceDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        });
    }

    private sealed record CheckoutApiResponse(
        [property: JsonPropertyName("reference")] string Reference,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("subtotalInr")] decimal SubtotalInr,
        [property: JsonPropertyName("totalInr")] decimal TotalInr);
}
