namespace Gaaunikh.Api.Data.Entities;

public sealed class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WeightLabel { get; set; } = string.Empty;
    public decimal UnitPriceInr { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotalInr { get; set; }
}
