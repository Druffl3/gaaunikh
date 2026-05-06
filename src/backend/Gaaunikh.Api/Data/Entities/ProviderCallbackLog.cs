namespace Gaaunikh.Api.Data.Entities;

public sealed class ProviderCallbackLog
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? ExternalEventId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public bool Processed { get; set; }
    public DateTimeOffset ReceivedUtc { get; set; }
}
