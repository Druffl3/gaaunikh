namespace Gaaunikh.Api.Features.Payments;

public sealed record VerifiedPaymentCallbackRequest(
    string RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature);
