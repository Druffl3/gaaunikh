using System.Text.Json;
using Gaaunikh.Api.Configuration;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Features.Orders;
using Gaaunikh.Api.Features.Payments;
using Gaaunikh.Api.Infrastructure.Payments;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Gaaunikh.Api.Tests")]

var builder = WebApplication.CreateBuilder(args);
var commerceConnectionString = builder.Configuration.GetConnectionString("CommerceDatabase")
    ?? throw new InvalidOperationException("Missing CommerceDatabase connection string.");

builder.Services.Configure<CommerceOptions>(builder.Configuration);
builder.Services.AddDbContext<CommerceDbContext>(options => options.UseNpgsql(commerceConnectionString));
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IRazorpayGateway, RazorpayGateway>();

var app = builder.Build();
var catalogProducts = CatalogSeedData.Products;
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthResponse("healthy", DateTimeOffset.UtcNow));
});

app.MapGet("/api/catalog/products", (string? search, string? category) =>
{
    IEnumerable<CatalogProduct> query = catalogProducts;

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(product =>
            product.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(product =>
            product.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    var items = query
        .Select(product => new CatalogProductListItem(
            product.Slug,
            product.Name,
            product.Category,
            product.ShortDescription,
            product.Variants.Min(variant => variant.PriceInr),
            product.Variants.Max(variant => variant.PriceInr)))
        .ToArray();

    return Results.Ok(new CatalogProductsResponse(items));
});

app.MapGet("/api/catalog/products/{slug}", (string slug) =>
{
    var product = catalogProducts.FirstOrDefault(item =>
        item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    if (product is null)
    {
        return Results.NotFound(new { error = "product_not_found" });
    }

    return Results.Ok(product);
});

app.MapPost("/api/orders/checkout", async (CheckoutRequest request, OrderService orderService, CancellationToken cancellationToken) =>
{
    try
    {
        var response = await orderService.CreateAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (CheckoutValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/payments/create-payment", async (CreatePaymentRequest request, PaymentService paymentService, CancellationToken cancellationToken) =>
{
    try
    {
        var response = await paymentService.CreateAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (PaymentValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/payments/razorpay/callback", async (HttpRequest request, PaymentService paymentService, CancellationToken cancellationToken) =>
{
    var rawPayload = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
    var callback = JsonSerializer.Deserialize<VerifiedPaymentCallbackRequest>(rawPayload, jsonOptions);

    if (callback is null)
    {
        return Results.BadRequest(new { error = "invalid_callback_payload" });
    }

    try
    {
        var response = await paymentService.HandleCallbackAsync(callback, rawPayload, cancellationToken);
        return response.Verified
            ? Results.Ok(response)
            : Results.BadRequest(new { error = "invalid_signature" });
    }
    catch (PaymentValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/payments/razorpay/webhook", async (HttpRequest request, PaymentService paymentService, CancellationToken cancellationToken) =>
{
    var rawPayload = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);

    try
    {
        var response = await paymentService.HandleWebhookAsync(
            rawPayload,
            request.Headers["X-Razorpay-Signature"],
            cancellationToken);

        return response.Verified
            ? Results.Ok(response)
            : Results.BadRequest(new { error = "invalid_signature" });
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "invalid_webhook_payload" });
    }
});

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var resolvedFrontendPath = FrontendStaticPageResolver.Resolve(app.Environment.WebRootPath, context.Request.Path);
    if (resolvedFrontendPath is null)
    {
        var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
        if (!File.Exists(indexPath))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = "frontend_not_built" });
            return;
        }

        resolvedFrontendPath = indexPath;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(resolvedFrontendPath);
});

app.Run();

internal sealed record HealthResponse(string Status, DateTimeOffset TimestampUtc);
internal sealed record CatalogProductsResponse(IReadOnlyList<CatalogProductListItem> Products);
internal sealed record CatalogProductListItem(
    string Slug,
    string Name,
    string Category,
    string ShortDescription,
    decimal LowestPriceInr,
    decimal HighestPriceInr);
internal sealed record CatalogProduct(
    string Slug,
    string Name,
    string Category,
    string ShortDescription,
    string Description,
    IReadOnlyList<CatalogVariant> Variants);
internal sealed record CatalogVariant(string WeightLabel, decimal PriceInr);

internal static class FrontendStaticPageResolver
{
    public static string? Resolve(string webRootPath, PathString requestPath)
    {
        if (string.IsNullOrWhiteSpace(webRootPath) || !Directory.Exists(webRootPath))
        {
            return null;
        }

        var requestValue = requestPath.Value ?? "/";
        var relativePath = requestValue.TrimStart('/');

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return FindExistingFile(webRootPath, ["index.html"]);
        }

        if (relativePath.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

        return FindExistingFile(
            webRootPath,
            [
                normalizedRelativePath,
                $"{normalizedRelativePath}.html",
                Path.Combine(normalizedRelativePath, "index.html")
            ]);
    }

    private static string? FindExistingFile(string webRootPath, IEnumerable<string> relativeCandidates)
    {
        foreach (var candidate in relativeCandidates)
        {
            var fullPath = Path.Combine(webRootPath, candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
}

internal static class CatalogSeedData
{
    public static IReadOnlyList<CatalogProduct> Products { get; } =
    [
        new(
            "kashmiri-chili-powder",
            "Kashmiri Chili Powder",
            "Single Spice",
            "Bright color, balanced warmth, and layered aroma.",
            "Crafted from low-heat Kashmiri chilies to deliver color-first flavor for curries, tandoori marinades, and finishing oils.",
            [
                new("100g", 95m),
                new("250g", 210m),
                new("500g", 395m)
            ]),
        new(
            "haldi-gold-turmeric",
            "Haldi Gold Turmeric",
            "Single Spice",
            "Fresh turmeric brightness for daily cooking.",
            "Stone-milled turmeric root with deep golden tone and warm earthy finish for dals, sabzis, and wellness recipes.",
            [
                new("100g", 80m),
                new("250g", 175m),
                new("500g", 330m)
            ]),
        new(
            "roasted-coriander-powder",
            "Roasted Coriander Powder",
            "Single Spice",
            "Citrus-lifted coriander with roasted depth.",
            "Slow-roasted coriander seeds milled in small batches for bright, nutty character in gravies, chutneys, and dry rubs.",
            [
                new("100g", 72m),
                new("250g", 158m),
                new("500g", 295m)
            ]),
        new(
            "signature-garam-masala",
            "Signature Garam Masala",
            "House Blend",
            "Warm whole-spice blend for finishing dishes.",
            "A balanced house blend of cardamom, clove, cinnamon, and pepper designed to lift aroma in North and South Indian dishes.",
            [
                new("100g", 120m),
                new("250g", 265m),
                new("500g", 510m)
            ]),
        new(
            "coastal-kitchen-blend",
            "Coastal Kitchen Blend",
            "House Blend",
            "Peppery-red blend for seafood and vegetable fries.",
            "A robust red masala with black pepper, chili, garlic, and curry leaf notes suitable for coastal gravies and pan-seared vegetables.",
            [
                new("100g", 130m),
                new("250g", 288m),
                new("500g", 548m)
            ])
    ];
}

public partial class Program;
