using Nebula.Application.Common;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;

namespace Nebula.Application.Services;

public class CommunicationEventService(
    ICommunicationEventRepository commRepo,
    ITaskRepository taskRepo,
    IUnitOfWork unitOfWork,
    ITimelineRepository timelineRepo)
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromHours(24);

    public async Task<(CommunicationEventSummaryDto? Dto, string? Error)> LogAsync(
        CommunicationLogRequest req, ICurrentUserService user, CancellationToken ct = default)
    {
        if (!await commRepo.EntityExistsAsync(req.PrimaryEntityType, req.PrimaryEntityId, ct))
            return (null, "entity_not_found");

        var now = DateTime.UtcNow;
        var comm = new CommunicationEvent
        {
            Id = Guid.NewGuid(),
            EventType = req.EventType,
            Direction = req.Direction,
            DurationMinutes = req.DurationMinutes,
            Subject = req.Subject,
            Body = req.Body,
            Outcome = req.Outcome,
            OccurredAt = req.OccurredAt ?? now,
            PrimaryEntityType = req.PrimaryEntityType,
            PrimaryEntityId = req.PrimaryEntityId,
            AuthoredByUserId = user.UserId,
            AuthoredByDisplayName = user.DisplayName ?? "Unknown",
            CreatedAt = now,
            CreatedByUserId = user.UserId,
            UpdatedAt = now,
            UpdatedByUserId = user.UserId,
        };

        await commRepo.AddAsync(comm, ct);

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            Id = Guid.NewGuid(),
            EntityType = req.PrimaryEntityType,
            EntityId = req.PrimaryEntityId,
            EventType = "CommunicationLogged",
            EventPayloadJson = $"{{\"communicationId\":\"{comm.Id}\",\"eventType\":\"{req.EventType}\"}}",
            EventDescription = $"{req.EventType} logged by {comm.AuthoredByDisplayName}",
            BrokerDescription = null,
            ActorUserId = user.UserId,
            ActorDisplayName = comm.AuthoredByDisplayName,
            OccurredAt = now,
        }, ct);

        await unitOfWork.CommitAsync(ct);

        return (MapToSummary(comm, user, null), null);
    }

    public async Task<(PaginatedCommunicationListDto? Dto, string? Error)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize,
        ICurrentUserService user, CancellationToken ct = default)
    {
        if (!await commRepo.EntityExistsAsync(entityType, entityId, ct))
            return (null, "entity_not_found");

        var effectivePage = Math.Max(1, page);
        var effectiveSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await commRepo.ListByEntityAsync(entityType, entityId, effectivePage, effectiveSize, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)effectiveSize);

        var followUpTasks = await LoadFollowUpTasksAsync(items, ct);

        var data = items
            .Select(e => MapToSummary(e, user, followUpTasks.GetValueOrDefault(e.FollowUpTaskId ?? Guid.Empty)))
            .ToList();

        return (new PaginatedCommunicationListDto(data, effectivePage, effectiveSize, totalCount, totalPages), null);
    }

    public async Task<(CommunicationEventSummaryDto? Dto, string? Error)> GetByIdAsync(
        Guid id, ICurrentUserService user, CancellationToken ct = default)
    {
        var comm = await commRepo.GetByIdAsync(id, ct);
        if (comm is null)
            return (null, "not_found");

        TaskItem? followUpTask = comm.FollowUpTaskId.HasValue
            ? await taskRepo.GetByIdAsync(comm.FollowUpTaskId.Value, ct)
            : null;

        return (MapToSummary(comm, user, followUpTask), null);
    }

    public async Task<(CommunicationEventSummaryDto? Dto, string? Error)> EditAsync(
        Guid id, CommunicationEditRequest req, ICurrentUserService user, CancellationToken ct = default)
    {
        if (req.Body is null && req.Subject is null && req.Outcome is null && req.OccurredAt is null)
            return (null, "no_changes");

        var comm = await commRepo.GetByIdAsync(id, ct);
        if (comm is null)
            return (null, "not_found");

        if (comm.IsRedacted)
            return (null, "event_redacted");

        var isAdmin = user.Roles.Contains("Admin");
        if (!isAdmin)
        {
            if (comm.AuthoredByUserId != user.UserId)
                return (null, "not_author");
            if (DateTime.UtcNow - comm.CreatedAt > EditWindow)
                return (null, "edit_window_expired");
        }

        var now = DateTime.UtcNow;
        if (req.Body is not null) comm.Body = req.Body;
        if (req.Subject is not null) comm.Subject = req.Subject;
        if (req.Outcome is not null) comm.Outcome = req.Outcome;
        if (req.OccurredAt is not null) comm.OccurredAt = req.OccurredAt.Value;

        comm.LastEditedAt = now;
        comm.LastEditedByUserId = user.UserId;
        comm.UpdatedAt = now;
        comm.UpdatedByUserId = user.UserId;

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            Id = Guid.NewGuid(),
            EntityType = comm.PrimaryEntityType,
            EntityId = comm.PrimaryEntityId,
            EventType = "CommunicationEdited",
            EventPayloadJson = $"{{\"communicationId\":\"{comm.Id}\"}}",
            EventDescription = $"{comm.EventType} edited by {user.DisplayName ?? "Unknown"}",
            BrokerDescription = null,
            ActorUserId = user.UserId,
            ActorDisplayName = user.DisplayName ?? "Unknown",
            OccurredAt = now,
        }, ct);

        await unitOfWork.CommitAsync(ct);

        TaskItem? followUpTask = comm.FollowUpTaskId.HasValue
            ? await taskRepo.GetByIdAsync(comm.FollowUpTaskId.Value, ct)
            : null;

        return (MapToSummary(comm, user, followUpTask), null);
    }

    public async Task<(CommunicationEventSummaryDto? Dto, string? Error)> RedactAsync(
        Guid id, CommunicationRedactRequest req, ICurrentUserService user, CancellationToken ct = default)
    {
        var comm = await commRepo.GetByIdAsync(id, ct);
        if (comm is null)
            return (null, "not_found");

        if (comm.IsRedacted)
            return (null, "already_redacted");

        var now = DateTime.UtcNow;
        comm.Body = null;
        comm.Outcome = null;
        comm.IsRedacted = true;
        comm.RedactedAt = now;
        comm.RedactedByUserId = user.UserId;
        comm.RedactedByDisplayName = user.DisplayName ?? "Unknown";
        comm.RedactionReason = req.Reason;
        comm.UpdatedAt = now;
        comm.UpdatedByUserId = user.UserId;

        await timelineRepo.AddEventAsync(new ActivityTimelineEvent
        {
            Id = Guid.NewGuid(),
            EntityType = comm.PrimaryEntityType,
            EntityId = comm.PrimaryEntityId,
            EventType = "CommunicationRedacted",
            EventPayloadJson = $"{{\"communicationId\":\"{comm.Id}\"}}",
            EventDescription = $"{comm.EventType} redacted by {comm.RedactedByDisplayName}",
            BrokerDescription = null,
            ActorUserId = user.UserId,
            ActorDisplayName = comm.RedactedByDisplayName,
            OccurredAt = now,
        }, ct);

        await unitOfWork.CommitAsync(ct);

        return (MapToSummary(comm, user, null), null);
    }

    public async Task<(TaskSummaryDto? Dto, string? Error)> CreateFollowUpAsync(
        Guid communicationId, CommunicationFollowUpRequest req, ICurrentUserService user, CancellationToken ct = default)
    {
        var comm = await commRepo.GetByIdAsync(communicationId, ct);
        if (comm is null)
            return (null, "not_found");

        if (comm.FollowUpTaskId.HasValue)
            return (null, "follow_up_exists");

        var assigneeId = req.AssignedToUserId ?? user.UserId;
        var now = DateTime.UtcNow;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Status = "Open",
            Priority = req.Priority ?? "Normal",
            DueDate = req.DueDate.HasValue ? req.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            AssignedToUserId = assigneeId,
            LinkedEntityType = "CommunicationEvent",
            LinkedEntityId = communicationId,
            CreatedAt = now,
            CreatedByUserId = user.UserId,
            UpdatedAt = now,
            UpdatedByUserId = user.UserId,
        };

        await taskRepo.AddAsync(task, ct);

        comm.FollowUpTaskId = task.Id;
        comm.UpdatedAt = now;
        comm.UpdatedByUserId = user.UserId;

        await unitOfWork.CommitAsync(ct);

        return (new TaskSummaryDto(
            task.Id, task.Title, task.Status, task.DueDate,
            task.LinkedEntityType, task.LinkedEntityId, null,
            false, null), null);
    }

    private async Task<Dictionary<Guid, TaskItem>> LoadFollowUpTasksAsync(
        IReadOnlyList<CommunicationEvent> items, CancellationToken ct)
    {
        var result = new Dictionary<Guid, TaskItem>();
        foreach (var item in items.Where(i => i.FollowUpTaskId.HasValue))
        {
            var taskId = item.FollowUpTaskId!.Value;
            if (!result.ContainsKey(taskId))
            {
                var t = await taskRepo.GetByIdAsync(taskId, ct);
                if (t is not null) result[taskId] = t;
            }
        }
        return result;
    }

    private static CommunicationEventSummaryDto MapToSummary(
        CommunicationEvent e, ICurrentUserService user, TaskItem? followUpTask)
    {
        var isAdmin = user.Roles.Contains("Admin");
        return new CommunicationEventSummaryDto(
            e.Id,
            e.EventType,
            e.Direction,
            e.DurationMinutes,
            e.Subject,
            e.Body,
            e.Outcome,
            e.OccurredAt,
            e.PrimaryEntityType,
            e.PrimaryEntityId,
            e.AuthoredByUserId,
            e.AuthoredByDisplayName,
            e.LastEditedAt,
            e.IsRedacted,
            e.RedactedByDisplayName,
            e.RedactedAt,
            isAdmin ? e.RedactionReason : null,
            e.FollowUpTaskId,
            followUpTask?.Title,
            followUpTask?.Status);
    }
}
