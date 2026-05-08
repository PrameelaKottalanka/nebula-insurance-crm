namespace Nebula.Application.DTOs;

public record CommunicationLogRequest(
    string EventType,
    string? Direction,
    int? DurationMinutes,
    string? Subject,
    string Body,
    string? Outcome,
    DateTime? OccurredAt,
    string PrimaryEntityType,
    Guid PrimaryEntityId);

public record CommunicationEditRequest(
    string? Body,
    string? Subject,
    string? Outcome,
    DateTime? OccurredAt);

public record CommunicationRedactRequest(string Reason);

public record CommunicationFollowUpRequest(
    string Title,
    DateOnly? DueDate,
    Guid? AssignedToUserId,
    string? Priority);

public record CommunicationEventSummaryDto(
    Guid Id,
    string EventType,
    string? Direction,
    int? DurationMinutes,
    string? Subject,
    string? Body,
    string? Outcome,
    DateTime OccurredAt,
    string PrimaryEntityType,
    Guid PrimaryEntityId,
    Guid AuthoredByUserId,
    string AuthoredByDisplayName,
    DateTime? LastEditedAt,
    bool IsRedacted,
    string? RedactedByDisplayName,
    DateTime? RedactedAt,
    string? RedactionReason,
    Guid? FollowUpTaskId,
    string? FollowUpTaskTitle,
    string? FollowUpTaskStatus);

public record PaginatedCommunicationListDto(
    IReadOnlyList<CommunicationEventSummaryDto> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
