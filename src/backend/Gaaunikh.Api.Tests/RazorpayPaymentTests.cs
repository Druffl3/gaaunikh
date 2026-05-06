using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Gaaunikh.Api.Infrastructure.Payments;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gaaunikh.Api.Tests;

public sealed class RazorpayPaymentTests
{
    [Fact]
    public async Task CreatePayment_CreatesRazorpayOrderForPendingOrder()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var orderId = await SeedPendingOrderAsync(factory.Services);

        var response = await client.PostAsJsonAsync("/api/payments/create-payment", new
        {
            orderId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CreatePaymentApiResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Razorpay", payload!.Provider);
        Assert.Equal("order_test_123", payload.RazorpayOrderId);
        Assert.Equal("rzp_test_placeholder", payload.RazorpayKeyId);
        Assert.Equal(190m, payload.AmountInr);
        Assert.Equal("INR", payload.Currency);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var order = await dbContext.Orders.Include(item => item.PaymentAttempts).SingleAsync(item => item.Id == orderId);
        var paymentAttempt = Assert.Single(order.PaymentAttempts);

        Assert.Equal("PendingPayment", order.Status);
        Assert.Equal("Created", paymentAttempt.Status);
        Assert.Equal("Razorpay", paymentAttempt.Provider);
        Assert.Equal("order_test_123", paymentAttempt.ProviderOrderId);
        Assert.Equal(190m, paymentAttempt.AmountInr);
    }

    [Fact]
    public async Task VerifiedCallback_RequiresValidSignature()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var orderId = await SeedPendingOrderAsync(factory.Services);
        await SeedPaymentAttemptAsync(factory.Services, orderId, "order_test_callback");

        var invalidResponse = await client.PostAsJsonAsync("/api/payments/razorpay/callback", new
        {
            razorpayOrderId = "order_test_callback",
            razorpayPaymentId = "pay_test_callback",
            razorpaySignature = "invalid-signature"
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        var validResponse = await client.PostAsJsonAsync("/api/payments/razorpay/callback", new
        {
            razorpayOrderId = "order_test_callback",
            razorpayPaymentId = "pay_test_callback",
            razorpaySignature = ComputeSignature("test_key_secret", "order_test_callback|pay_test_callback")
        });

        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var order = await dbContext.Orders.Include(item => item.PaymentAttempts).SingleAsync(item => item.Id == orderId);
        var paymentAttempt = Assert.Single(order.PaymentAttempts);
        var callbackLogs = await dbContext.ProviderCallbackLogs
            .Where(item => item.EventType == "payment.callback")
            .OrderBy(item => item.ReceivedUtc)
            .ToListAsync();

        Assert.Equal("PaymentAuthorized", order.Status);
        Assert.Equal("Authorized", paymentAttempt.Status);
        Assert.Equal("pay_test_callback", paymentAttempt.ProviderPaymentId);
        Assert.Equal(2, callbackLogs.Count);
        Assert.False(callbackLogs[0].Processed);
        Assert.True(callbackLogs[1].Processed);
    }

    [Fact]
    public async Task WebhookSignatureValidationIsRequired()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var orderId = await SeedPendingOrderAsync(factory.Services);
        await SeedPaymentAttemptAsync(factory.Services, orderId, "order_test_webhook");

        const string webhookPayload = """
            {
              "event": "payment.captured",
              "payload": {
                "payment": {
                  "entity": {
                    "id": "pay_test_webhook",
                    "order_id": "order_test_webhook",
                    "status": "captured"
                  }
                }
              }
            }
            """;

        using var invalidRequest = CreateWebhookRequest(webhookPayload, "invalid-signature");
        var invalidResponse = await client.SendAsync(invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        using var validRequest = CreateWebhookRequest(webhookPayload, ComputeSignature("test_webhook_secret", webhookPayload));
        var validResponse = await client.SendAsync(validRequest);

        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        var order = await dbContext.Orders.Include(item => item.PaymentAttempts).SingleAsync(item => item.Id == orderId);
        var paymentAttempt = Assert.Single(order.PaymentAttempts);
        var webhookLogs = await dbContext.ProviderCallbackLogs
            .Where(item => item.EventType == "payment.captured")
            .OrderBy(item => item.ReceivedUtc)
            .ToListAsync();

        Assert.Equal("Paid", order.Status);
        Assert.Equal("Captured", paymentAttempt.Status);
        Assert.Equal("pay_test_webhook", paymentAttempt.ProviderPaymentId);
        Assert.Equal(2, webhookLogs.Count);
        Assert.False(webhookLogs[0].Processed);
        Assert.True(webhookLogs[1].Processed);
    }

    [Fact]
    public async Task DuplicateWebhookPayloads_AreIdempotent()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var orderId = await SeedPendingOrderAsync(factory.Services);
        await SeedPaymentAttemptAsync(factory.Services, orderId, "order_test_duplicate");

        const string webhookPayload = """
            {
              "event": "payment.captured",
              "payload": {
                "payment": {
                  "entity": {
                    "id": "pay_test_duplicate",
                    "order_id": "order_test_duplicate",
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
        var order = await dbContext.Orders.Include(item => item.PaymentAttempts).SingleAsync(item => item.Id == orderId);
        var paymentAttempt = Assert.Single(order.PaymentAttempts);
        var webhookLogs = await dbContext.ProviderCallbackLogs
            .Where(item => item.EventType == "payment.captured" && item.ExternalEventId == "pay_test_duplicate")
            .ToListAsync();

        Assert.Equal("Paid", order.Status);
        Assert.Equal("Captured", paymentAttempt.Status);
        Assert.Equal("pay_test_duplicate", paymentAttempt.ProviderPaymentId);
        Assert.Equal(2, webhookLogs.Count);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"razorpay-tests-{Guid.NewGuid():N}";

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

    private static async Task<Guid> SeedPendingOrderAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Reference = "ORD-RAZORPAY",
            Status = "PendingPayment",
            CustomerName = "Asha Raman",
            CustomerEmail = "asha@example.com",
            CustomerPhone = "+919999999999",
            ShippingAddressLine1 = "12 Spice Market Road",
            ShippingCity = "Bengaluru",
            ShippingState = "Karnataka",
            ShippingPostalCode = "560001",
            ShippingCountryCode = "IN",
            SubtotalInr = 190m,
            TotalInr = 190m,
            CreatedUtc = DateTimeOffset.UtcNow,
            Lines =
            [
                new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductSlug = "kashmiri-chili-powder",
                    ProductName = "Kashmiri Chili Powder",
                    WeightLabel = "100g",
                    UnitPriceInr = 95m,
                    Quantity = 2,
                    LineTotalInr = 190m
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
            AmountInr = 190m,
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

    private sealed record CreatePaymentApiResponse(
        string Provider,
        decimal AmountInr,
        string Currency,
        string RazorpayOrderId,
        string RazorpayKeyId);
}
