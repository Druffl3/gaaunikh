namespace Gaaunikh.Api.Features.Inventory;

public static class InventoryReservationPolicy
{
    public const string ReservationMovementType = "Reservation";
    public const string StockAdjustmentMovementType = "StockAdjustment";
    public const string PaidOrderReservationReason = "PaidOrderReservation";

    public static bool ShouldReserve(string orderStatus)
    {
        return orderStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);
    }
}
