namespace Gaaunikh.Api.Features.Orders;

public sealed record CheckoutResponse(
    Guid OrderId,
    string Reference,
    string Status,
    decimal SubtotalInr,
    decimal TotalInr);
