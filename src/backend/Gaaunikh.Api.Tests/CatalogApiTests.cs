using System.Net.Http.Json;

namespace Gaaunikh.Api.Tests;

public sealed class CatalogApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListProducts_ReturnsSeededCatalog()
    {
        var response = await _client.GetAsync("/api/catalog/products");
        var payload = await response.Content.ReadFromJsonAsync<CatalogListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Products);
    }

    [Fact]
    public async Task ListProducts_FiltersBySearch()
    {
        var response = await _client.GetAsync("/api/catalog/products?search=chili");
        var payload = await response.Content.ReadFromJsonAsync<CatalogListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Products);
        Assert.All(payload.Products, p => Assert.Contains("chili", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProductBySlug_ReturnsProductWithVariants()
    {
        var response = await _client.GetAsync("/api/catalog/products/kashmiri-chili-powder");
        var payload = await response.Content.ReadFromJsonAsync<CatalogProductDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("kashmiri-chili-powder", payload!.Slug);
        Assert.NotEmpty(payload.Variants);
    }

    [Fact]
    public async Task GetProductBySlug_Returns404ForUnknownSlug()
    {
        var response = await _client.GetAsync("/api/catalog/products/not-a-real-product");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
