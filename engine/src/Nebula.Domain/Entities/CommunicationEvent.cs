namespace Nebula.Domain.Entities;

public class CommunicationEvent : BaseEntity
{
    public string EventType { get; set; } = default!;         // Note | Call | Meeting
    public string? Direction { get; set; }                     // Inbound | Outbound | Internal; null for Note
    public int? DurationMinutes { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }                          // null after redaction
    public string? Outcome { get; set; }
    public DateTime OccurredAt { get; set; }
    public string PrimaryEntityType { get; set; } = default!; // Broker | Account | Submission | Policy | Renewal
    public Guid PrimaryEntityId { get; set; }
    public Guid AuthoredByUserId { get; set; }
    public string AuthoredByDisplayName { get; set; } = default!;
    public DateTime? LastEditedAt { get; set; }
    public Guid? LastEditedByUserId { get; set; }
    public bool IsRedacted { get; set; }
    public DateTime? RedactedAt { get; set; }
    public Guid? RedactedByUserId { get; set; }
    public string? RedactedByDisplayName { get; set; }
    public string? RedactionReason { get; set; }
    public Guid? FollowUpTaskId { get; set; }
}
