namespace Gaaunikh.Api.Infrastructure.Payments;

public interface IRazorpayGateway
{
    Task<RazorpayOrderResult> CreateOrderAsync(RazorpayOrderRequest request, CancellationToken cancellationToken);
}

public sealed record RazorpayOrderRequest(
    string Receipt,
    decimal AmountInr,
    string Currency,
    bool AutoCapture);

public sealed record RazorpayOrderResult(
    string OrderId,
    string KeyId,
    decimal AmountInr,
    string Currency,
    bool AutoCapture);
