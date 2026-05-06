namespace Gaaunikh.Api.Data.Entities;

public sealed class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal AmountInr { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ProviderOrderId { get; set; }
    public string? ProviderPaymentId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
