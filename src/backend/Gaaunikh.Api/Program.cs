using System.Text.Json;
using Gaaunikh.Api.Configuration;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Features.Inventory;
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
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IRazorpayGateway, RazorpayGateway>();

var app = builder.Build();
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseInitialization");

    try
    {
        var providerName = dbContext.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        else if (dbContext.Database.IsRelational())
        {
            var migrations = dbContext.Database.GetMigrations();
            if (migrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Skipping startup database initialization.");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new HealthResponse("healthy", DateTimeOffset.UtcNow));
});

app.MapGet("/api/catalog/products", async (string? search, string? category, InventoryService inventoryService, CancellationToken cancellationToken) =>
{
    var items = await inventoryService.GetCatalogProductsAsync(search, category, cancellationToken);

    return Results.Ok(new CatalogProductsResponse(items));
});

app.MapGet("/api/catalog/products/{slug}", async (string slug, InventoryService inventoryService, CancellationToken cancellationToken) =>
{
    var product = await inventoryService.GetCatalogProductAsync(slug, cancellationToken);

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

app.MapGet("/api/admin/inventory/summary", async (
    string? search,
    string? category,
    string? sku,
    InventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    var items = await inventoryService.GetSummaryAsync(search, category, sku, cancellationToken);
    return Results.Ok(new InventorySummaryResponse(items));
});

app.MapPost("/api/admin/inventory/items", async (
    CreateInventoryItemRequest request,
    InventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var item = await inventoryService.CreateItemAsync(request, cancellationToken);
        return Results.Ok(item);
    }
    catch (InventoryValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/admin/inventory/adjustments", async (
    StockAdjustmentRequest request,
    InventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var item = await inventoryService.AdjustStockAsync(request, cancellationToken);
        return Results.Ok(item);
    }
    catch (InventoryValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
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
internal sealed record CatalogProductsResponse(IReadOnlyList<CatalogProductListItemDto> Products);

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

public partial class Program;
