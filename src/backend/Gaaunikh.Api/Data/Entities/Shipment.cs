namespace Gaaunikh.Api.Data.Entities;

public sealed class Shipment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProviderShipmentId { get; set; }
    public string? Awb { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
