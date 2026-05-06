namespace Gaaunikh.Api.Configuration;

public sealed class CommerceOptions
{
    public PaymentsOptions Payments { get; init; } = new();
    public ShippingOptions Shipping { get; init; } = new();
    public NotificationsOptions Notifications { get; init; } = new();
}

public sealed class PaymentsOptions
{
    public string Provider { get; init; } = string.Empty;
    public string Currency { get; init; } = "INR";
    public RazorpayOptions Razorpay { get; init; } = new();
}

public sealed class RazorpayOptions
{
    public string KeyId { get; init; } = string.Empty;
    public string KeySecret { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public bool AutoCapture { get; init; }
}

public sealed class ShippingOptions
{
    public string Provider { get; init; } = string.Empty;
    public ShiprocketOptions Shiprocket { get; init; } = new();
}

public sealed class ShiprocketOptions
{
    public string ApiBaseUrl { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string DefaultPickupLocation { get; init; } = string.Empty;
    public decimal DefaultPackageLengthCm { get; init; }
    public decimal DefaultPackageWidthCm { get; init; }
    public decimal DefaultPackageHeightCm { get; init; }
    public decimal DefaultPackageWeightKg { get; init; }
    public int TrackingPollIntervalMinutes { get; init; }
}

public sealed class NotificationsOptions
{
    public EmailNotificationsOptions Email { get; init; } = new();
    public WhatsAppNotificationsOptions WhatsApp { get; init; } = new();
}

public sealed class EmailNotificationsOptions
{
    public string Provider { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string ReplyToAddress { get; init; } = string.Empty;
    public string AdminAlertAddress { get; init; } = string.Empty;
    public ResendOptions Resend { get; init; } = new();
}

public sealed class ResendOptions
{
    public string ApiKey { get; init; } = string.Empty;
}

public sealed class WhatsAppNotificationsOptions
{
    public string Provider { get; init; } = string.Empty;
    public WhatsAppTemplateOptions Templates { get; init; } = new();
    public TwilioOptions Twilio { get; init; } = new();
}

public sealed class WhatsAppTemplateOptions
{
    public string OrderConfirmed { get; init; } = string.Empty;
    public string ShipmentCreated { get; init; } = string.Empty;
    public string ShipmentDelivered { get; init; } = string.Empty;
}

public sealed class TwilioOptions
{
    public string AccountSid { get; init; } = string.Empty;
    public string AuthToken { get; init; } = string.Empty;
    public string FromNumber { get; init; } = string.Empty;
    public string StatusCallbackSecret { get; init; } = string.Empty;
}
