using System.Net.Http.Json;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gaaunikh.Api.Tests;

public sealed class CatalogApiTests
{
    [Fact]
    public async Task ListProducts_ReturnsOnlyInventoryCreatedCatalog()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await SeedInventoryItemAsync(factory.Services, CreateInventoryItem(
            sku: "SPICE-SMOKED-PAPRIKA-200G",
            productSlug: "smoked-paprika",
            productName: "Smoked Paprika",
            weightLabel: "200g",
            unitPriceInr: 120m));

        var response = await client.GetAsync("/api/catalog/products");
        var payload = await response.Content.ReadFromJsonAsync<CatalogListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var product = Assert.Single(payload!.Products);
        Assert.Equal("smoked-paprika", product.Slug);
        Assert.Equal("Smoked Paprika", product.Name);
    }

    [Fact]
    public async Task ListProducts_FiltersBySearch()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await SeedInventoryItemsAsync(
            factory.Services,
            CreateInventoryItem(
                sku: "SPICE-SMOKED-PAPRIKA-200G",
                productSlug: "smoked-paprika",
                productName: "Smoked Paprika",
                weightLabel: "200g",
                unitPriceInr: 120m),
            CreateInventoryItem(
                sku: "SPICE-ROASTED-CUMIN-100G",
                productSlug: "roasted-cumin",
                productName: "Roasted Cumin",
                weightLabel: "100g",
                unitPriceInr: 88m));

        var response = await client.GetAsync("/api/catalog/products?search=paprika");
        var payload = await response.Content.ReadFromJsonAsync<CatalogListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        var product = Assert.Single(payload!.Products);
        Assert.Contains("paprika", product.Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProductBySlug_ReturnsProductWithInventoryBackedVariants()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        await SeedInventoryItemsAsync(
            factory.Services,
            CreateInventoryItem(
                sku: "BLEND-FIRE-MASALA-100G",
                productSlug: "fire-roast-masala",
                productName: "Fire Roast Masala",
                weightLabel: "100g",
                unitPriceInr: 140m),
            CreateInventoryItem(
                sku: "BLEND-FIRE-MASALA-250G",
                productSlug: "fire-roast-masala",
                productName: "Fire Roast Masala",
                weightLabel: "250g",
                unitPriceInr: 320m));

        var response = await client.GetAsync("/api/catalog/products/fire-roast-masala");
        var payload = await response.Content.ReadFromJsonAsync<CatalogProductDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("fire-roast-masala", payload!.Slug);
        Assert.Equal(2, payload.Variants.Count);
        Assert.Contains(payload.Variants, variant => variant.WeightLabel == "100g");
        Assert.Contains(payload.Variants, variant => variant.WeightLabel == "250g");
    }

    [Fact]
    public async Task GetProductBySlug_Returns404ForUnknownSlug()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/catalog/products/not-a-real-product");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"catalog-tests-{Guid.NewGuid():N}";

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

    private static InventoryItem CreateInventoryItem(
        string sku,
        string productSlug,
        string productName,
        string weightLabel,
        decimal unitPriceInr)
    {
        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            ProductSlug = productSlug,
            ProductName = productName,
            Category = "Single Spice",
            ShortDescription = $"{productName} short description.",
            Description = $"{productName} detailed description.",
            WeightLabel = weightLabel,
            UnitPriceInr = unitPriceInr,
            ReorderThreshold = 3,
            IsActive = true,
            CreatedUtc = DateTimeOffset.UtcNow,
            UpdatedUtc = DateTimeOffset.UtcNow
        };
    }

    private static async Task SeedInventoryItemAsync(IServiceProvider services, InventoryItem item)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedInventoryItemsAsync(IServiceProvider services, params InventoryItem[] items)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        dbContext.InventoryItems.AddRange(items);
        await dbContext.SaveChangesAsync();
    }

    public sealed record CatalogListResponse(
        [property: JsonPropertyName("products")] IReadOnlyList<CatalogProductListItem> Products);

    public sealed record CatalogProductListItem(
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("name")] string Name);

    public sealed record CatalogProductDetailsResponse(
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("variants")] IReadOnlyList<CatalogVariant> Variants);

    public sealed record CatalogVariant(
        [property: JsonPropertyName("weightLabel")] string WeightLabel,
        [property: JsonPropertyName("priceInr")] decimal PriceInr);
}
