using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gaaunikh.Api.Tests;

public sealed class DatabaseStartupTests
{
    [Fact]
    public async Task Checkout_OnFreshRelationalDatabase_BootstrapsSchemaBeforeSaving()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CommerceDbContext>>();
                services.RemoveAll<CommerceDbContext>();
                services.AddDbContext<CommerceDbContext>(options => options.UseSqlite(connection));
            });
        });

        using var client = factory.CreateClient();
        await SeedInventoryItemAsync(factory.Services);

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
                    productSlug = "smoked-paprika",
                    weightLabel = "200g",
                    unitPriceInr = 120m,
                    quantity = 1
                }
            }
        });

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, responseBody);
    }

    private static async Task SeedInventoryItemAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();

        dbContext.InventoryItems.Add(new InventoryItem
        {
            Id = Guid.NewGuid(),
            Sku = "SPICE-SMOKED-PAPRIKA-200G",
            ProductSlug = "smoked-paprika",
            ProductName = "Smoked Paprika",
            Category = "Single Spice",
            ShortDescription = "Deep red smoked chili powder.",
            Description = "Bold smoked paprika for marinades and finishing spice blends.",
            WeightLabel = "200g",
            UnitPriceInr = 120m,
            ReorderThreshold = 3,
            IsActive = true,
            CreatedUtc = DateTimeOffset.UtcNow,
            UpdatedUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
