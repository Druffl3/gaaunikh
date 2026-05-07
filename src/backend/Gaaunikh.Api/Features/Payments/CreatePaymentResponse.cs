namespace Gaaunikh.Api.Features.Payments;

public sealed record CreatePaymentResponse(
    Guid OrderId,
    string OrderReference,
    string OrderStatus,
    Guid PaymentAttemptId,
    string Provider,
    decimal AmountInr,
    string Currency,
    string RazorpayOrderId,
    string RazorpayKeyId,
    bool AutoCapture);
