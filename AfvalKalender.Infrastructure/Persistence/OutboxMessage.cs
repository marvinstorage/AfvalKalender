using System;

namespace AfvalKalender.Infrastructure.Persistence;

public class OutboxMessage
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}
