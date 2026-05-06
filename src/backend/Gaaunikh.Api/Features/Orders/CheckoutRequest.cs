namespace Gaaunikh.Api.Features.Orders;

public sealed record CheckoutRequest(
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    CheckoutShippingAddress ShippingAddress,
    IReadOnlyList<CheckoutLineRequest> Lines);

public sealed record CheckoutShippingAddress(
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string CountryCode);

public sealed record CheckoutLineRequest(
    string ProductSlug,
    string WeightLabel,
    decimal UnitPriceInr,
    int Quantity);
