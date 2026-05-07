using Gaaunikh.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Gaaunikh.Api.Infrastructure.Payments;

public sealed class RazorpayGateway : IRazorpayGateway
{
    private readonly PaymentsOptions _paymentsOptions;

    public RazorpayGateway(IOptions<CommerceOptions> options)
    {
        _paymentsOptions = options.Value.Payments;
    }

    public Task<RazorpayOrderResult> CreateOrderAsync(RazorpayOrderRequest request, CancellationToken cancellationToken)
    {
        var keyId = string.IsNullOrWhiteSpace(_paymentsOptions.Razorpay.KeyId)
            ? "rzp_test_placeholder"
            : _paymentsOptions.Razorpay.KeyId;

        var result = new RazorpayOrderResult(
            $"order_{Guid.NewGuid():N}",
            keyId,
            request.AmountInr,
            string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency,
            request.AutoCapture);

        return Task.FromResult(result);
    }
}
