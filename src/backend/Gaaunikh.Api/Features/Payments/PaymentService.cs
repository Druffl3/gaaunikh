using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gaaunikh.Api.Configuration;
using Gaaunikh.Api.Data;
using Gaaunikh.Api.Data.Entities;
using Gaaunikh.Api.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gaaunikh.Api.Features.Payments;

public sealed class PaymentService
{
    private readonly CommerceDbContext _dbContext;
    private readonly IRazorpayGateway _razorpayGateway;
    private readonly PaymentsOptions _paymentsOptions;

    public PaymentService(
        CommerceDbContext dbContext,
        IRazorpayGateway razorpayGateway,
        IOptions<CommerceOptions> options)
    {
        _dbContext = dbContext;
        _razorpayGateway = razorpayGateway;
        _paymentsOptions = options.Value.Payments;
    }

    public async Task<CreatePaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(item => item.PaymentAttempts)
            .SingleOrDefaultAsync(item => item.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new PaymentValidationException("Unknown order.");
        }

        if (order.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentValidationException("Order is already paid.");
        }

        var existingAttempt = order.PaymentAttempts
            .OrderByDescending(item => item.CreatedUtc)
            .FirstOrDefault(item =>
                item.Provider.Equals("Razorpay", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(item.ProviderOrderId) &&
                !item.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));

        if (existingAttempt is not null)
        {
            return MapCreatePaymentResponse(order, existingAttempt);
        }

        var razorpayOrder = await _razorpayGateway.CreateOrderAsync(
            new RazorpayOrderRequest(
                order.Reference,
                order.TotalInr,
                string.IsNullOrWhiteSpace(_paymentsOptions.Currency) ? "INR" : _paymentsOptions.Currency,
                _paymentsOptions.Razorpay.AutoCapture),
            cancellationToken);

        var paymentAttempt = new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Provider = "Razorpay",
            Status = "Created",
            AmountInr = razorpayOrder.AmountInr,
            Currency = razorpayOrder.Currency,
            ProviderOrderId = razorpayOrder.OrderId,
            CreatedUtc = DateTimeOffset.UtcNow
        };

        _dbContext.PaymentAttempts.Add(paymentAttempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePaymentResponse(
            order.Id,
            order.Reference,
            order.Status,
            paymentAttempt.Id,
            paymentAttempt.Provider,
            paymentAttempt.AmountInr,
            paymentAttempt.Currency,
            paymentAttempt.ProviderOrderId ?? string.Empty,
            razorpayOrder.KeyId,
            razorpayOrder.AutoCapture);
    }

    public async Task<PaymentCallbackResponse> HandleCallbackAsync(
        VerifiedPaymentCallbackRequest request,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        var paymentAttempt = await _dbContext.PaymentAttempts
            .Include(item => item.Order)
            .SingleOrDefaultAsync(item => item.ProviderOrderId == request.RazorpayOrderId, cancellationToken);

        if (paymentAttempt is null || paymentAttempt.Order is null)
        {
            throw new PaymentValidationException("Unknown Razorpay order.");
        }

        var callbackLog = new ProviderCallbackLog
        {
            Id = Guid.NewGuid(),
            Provider = "Razorpay",
            EventType = "payment.callback",
            ExternalEventId = request.RazorpayPaymentId,
            PayloadJson = rawPayload,
            Processed = false,
            ReceivedUtc = DateTimeOffset.UtcNow
        };
        _dbContext.ProviderCallbackLogs.Add(callbackLog);

        var expectedSignature = ComputeSignature(
            _paymentsOptions.Razorpay.KeySecret,
            $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}");

        if (!FixedTimeEquals(expectedSignature, request.RazorpaySignature))
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new PaymentCallbackResponse(false, paymentAttempt.Status, paymentAttempt.Order.Status);
        }

        paymentAttempt.ProviderPaymentId = request.RazorpayPaymentId;
        if (!paymentAttempt.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase))
        {
            paymentAttempt.Status = "Authorized";
        }

        if (!paymentAttempt.Order.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            paymentAttempt.Order.Status = "PaymentAuthorized";
        }

        callbackLog.Processed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentCallbackResponse(true, paymentAttempt.Status, paymentAttempt.Order.Status);
    }

    public async Task<RazorpayWebhookResponse> HandleWebhookAsync(
        string rawPayload,
        string? signature,
        CancellationToken cancellationToken)
    {
        var webhook = ParseWebhook(rawPayload);
        var hasProcessedDuplicate = !string.IsNullOrWhiteSpace(webhook.ExternalEventId) &&
            await _dbContext.ProviderCallbackLogs.AnyAsync(
                item =>
                    item.Provider == "Razorpay" &&
                    item.EventType == webhook.EventType &&
                    item.ExternalEventId == webhook.ExternalEventId &&
                    item.Processed,
                cancellationToken);

        var callbackLog = new ProviderCallbackLog
        {
            Id = Guid.NewGuid(),
            Provider = "Razorpay",
            EventType = webhook.EventType,
            ExternalEventId = webhook.ExternalEventId,
            PayloadJson = rawPayload,
            Processed = false,
            ReceivedUtc = DateTimeOffset.UtcNow
        };
        _dbContext.ProviderCallbackLogs.Add(callbackLog);

        var expectedSignature = ComputeSignature(_paymentsOptions.Razorpay.WebhookSecret, rawPayload);
        if (!FixedTimeEquals(expectedSignature, signature))
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new RazorpayWebhookResponse(false, false, webhook.EventType);
        }

        if (hasProcessedDuplicate)
        {
            callbackLog.Processed = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new RazorpayWebhookResponse(true, true, webhook.EventType);
        }

        if (webhook.EventType == "payment.captured" && !string.IsNullOrWhiteSpace(webhook.ProviderOrderId))
        {
            var paymentAttempt = await _dbContext.PaymentAttempts
                .Include(item => item.Order)
                .SingleOrDefaultAsync(item => item.ProviderOrderId == webhook.ProviderOrderId, cancellationToken);

            if (paymentAttempt is not null)
            {
                paymentAttempt.ProviderPaymentId = webhook.ExternalEventId;
                paymentAttempt.Status = "Captured";

                if (paymentAttempt.Order is not null)
                {
                    paymentAttempt.Order.Status = "Paid";
                }
            }
        }

        callbackLog.Processed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RazorpayWebhookResponse(true, false, webhook.EventType);
    }

    private CreatePaymentResponse MapCreatePaymentResponse(Order order, PaymentAttempt paymentAttempt)
    {
        var keyId = string.IsNullOrWhiteSpace(_paymentsOptions.Razorpay.KeyId)
            ? "rzp_test_placeholder"
            : _paymentsOptions.Razorpay.KeyId;

        return new CreatePaymentResponse(
            order.Id,
            order.Reference,
            order.Status,
            paymentAttempt.Id,
            paymentAttempt.Provider,
            paymentAttempt.AmountInr,
            paymentAttempt.Currency,
            paymentAttempt.ProviderOrderId ?? string.Empty,
            keyId,
            _paymentsOptions.Razorpay.AutoCapture);
    }

    private static RazorpayWebhookPayload ParseWebhook(string rawPayload)
    {
        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;

        var eventType = root.TryGetProperty("event", out var eventElement)
            ? eventElement.GetString() ?? "unknown"
            : "unknown";

        var externalEventId = TryGetNestedString(root, "payload", "payment", "entity", "id");
        var providerOrderId = TryGetNestedString(root, "payload", "payment", "entity", "order_id");

        return new RazorpayWebhookPayload(eventType, externalEventId, providerOrderId);
    }

    private static string? TryGetNestedString(JsonElement root, params string[] path)
    {
        var current = root;

        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
    }

    private static string ComputeSignature(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret ?? string.Empty);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string expectedSignature, string? providedSignature)
    {
        if (string.IsNullOrWhiteSpace(providedSignature))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        var providedBytes = Encoding.UTF8.GetBytes(providedSignature.Trim());
        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private sealed record RazorpayWebhookPayload(string EventType, string? ExternalEventId, string? ProviderOrderId);
}

public sealed record PaymentCallbackResponse(bool Verified, string PaymentStatus, string OrderStatus);

public sealed record RazorpayWebhookResponse(bool Verified, bool Duplicate, string EventType);

public sealed class PaymentValidationException : Exception
{
    public PaymentValidationException(string message)
        : base(message)
    {
    }
}
