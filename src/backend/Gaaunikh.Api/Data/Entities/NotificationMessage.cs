namespace Gaaunikh.Api.Data.Entities;

public sealed class NotificationMessage
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProviderMessageId { get; set; }
    public string? LastError { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
