namespace Gaaunikh.Api.Data.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountryCode { get; set; } = string.Empty;
    public decimal SubtotalInr { get; set; }
    public decimal TotalInr { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
    public List<PaymentAttempt> PaymentAttempts { get; set; } = [];
    public List<Shipment> Shipments { get; set; } = [];
}
