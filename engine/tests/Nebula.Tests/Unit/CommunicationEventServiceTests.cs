using Shouldly;
using Nebula.Application.DTOs;
using Nebula.Application.Interfaces;
using Nebula.Application.Services;
using Nebula.Domain.Entities;

namespace Nebula.Tests.Unit;

public class CommunicationEventServiceTests
{
    private readonly StubCommunicationEventRepository _commRepo = new();
    private readonly StubTaskRepository _taskRepo = new();
    private readonly StubTimelineRepository _timelineRepo = new();
    private readonly StubUnitOfWork _unitOfWork = new();

    private readonly StubCurrentUserService _admin = new(
        Guid.Parse("aaaa0001-0000-0000-0000-000000000001"), roles: ["Admin"]);
    private readonly StubCurrentUserService _author = new(
        Guid.Parse("bbbb0001-0000-0000-0000-000000000001"), roles: ["DistributionUser"]);
    private readonly StubCurrentUserService _otherUser = new(
        Guid.Parse("cccc0001-0000-0000-0000-000000000001"), roles: ["DistributionUser"]);

    private const string EntityType = "Broker";
    private readonly Guid _entityId = Guid.Parse("dddd0001-0000-0000-0000-000000000001");

    private CommunicationEventService CreateService() =>
        new(_commRepo, _taskRepo, _unitOfWork, _timelineRepo);

    private void SeedEntity() => _commRepo.SeedEntity(EntityType, _entityId);

    private CommunicationEvent SeedEvent(
        Guid? authorId = null,
        bool isRedacted = false,
        DateTime? createdAt = null,
        Guid? followUpTaskId = null)
    {
        var now = DateTime.UtcNow;
        var comm = new CommunicationEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Note",
            Body = "Original body",
            Outcome = "Original outcome",
            OccurredAt = now,
            PrimaryEntityType = EntityType,
            PrimaryEntityId = _entityId,
            AuthoredByUserId = authorId ?? _author.UserId,
            AuthoredByDisplayName = "Test Author",
            IsRedacted = isRedacted,
            FollowUpTaskId = followUpTaskId,
            CreatedAt = createdAt ?? now,
            UpdatedAt = now,
            CreatedByUserId = authorId ?? _author.UserId,
            UpdatedByUserId = authorId ?? _author.UserId,
        };
        _commRepo.Seed(comm);
        return comm;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  LogAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task LogAsync_ValidNote_ReturnsDtoAndCommitsUoW()
    {
        SeedEntity();
        var svc = CreateService();
        var req = new CommunicationLogRequest("Note", null, null, "Test subject",
            "Test body", null, null, EntityType, _entityId);

        var (dto, error) = await svc.LogAsync(req, _author);

        error.ShouldBeNull();
        dto.ShouldNotBeNull();
        dto!.EventType.ShouldBe("Note");
        dto.Subject.ShouldBe("Test subject");
        dto.Body.ShouldBe("Test body");
        dto.AuthoredByUserId.ShouldBe(_author.UserId);
        _commRepo.Added.Count.ShouldBe(1);
        _unitOfWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task LogAsync_ValidCall_ReturnsDtoWithDirectionAndDuration()
    {
        SeedEntity();
        var svc = CreateService();
        var req = new CommunicationLogRequest("Call", "Inbound", 30, "Call subject",
            "Call body", "Positive", null, EntityType, _entityId);

        var (dto, error) = await svc.LogAsync(req, _author);

        error.ShouldBeNull();
        dto!.Direction.ShouldBe("Inbound");
        dto.DurationMinutes.ShouldBe(30);
    }

    [Fact]
    public async Task LogAsync_EntityNotFound_ReturnsError()
    {
        var svc = CreateService();
        var req = new CommunicationLogRequest("Note", null, null, null,
            "Body", null, null, EntityType, Guid.NewGuid());

        var (dto, error) = await svc.LogAsync(req, _author);

        error.ShouldBe("entity_not_found");
        dto.ShouldBeNull();
        _commRepo.Added.ShouldBeEmpty();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task LogAsync_EmitsTimelineEvent()
    {
        SeedEntity();
        var svc = CreateService();
        var req = new CommunicationLogRequest("Meeting", "Outbound", 60, null,
            "Meeting body", null, null, EntityType, _entityId);

        await svc.LogAsync(req, _author);

        _timelineRepo.Events.Count.ShouldBe(1);
        _timelineRepo.Events[0].EventType.ShouldBe("CommunicationLogged");
        _timelineRepo.Events[0].EntityType.ShouldBe(EntityType);
        _timelineRepo.Events[0].EntityId.ShouldBe(_entityId);
        _timelineRepo.Events[0].ActorUserId.ShouldBe(_author.UserId);
    }

    [Fact]
    public async Task LogAsync_OccurredAtDefaultsToNow_WhenNotProvided()
    {
        SeedEntity();
        var svc = CreateService();
        var before = DateTime.UtcNow;
        var req = new CommunicationLogRequest("Note", null, null, null,
            "Body", null, null, EntityType, _entityId);

        var (dto, _) = await svc.LogAsync(req, _author);

        dto!.OccurredAt.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public async Task LogAsync_OccurredAtUsedWhenProvided()
    {
        SeedEntity();
        var svc = CreateService();
        var pastDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var req = new CommunicationLogRequest("Note", null, null, null,
            "Body", null, pastDate, EntityType, _entityId);

        var (dto, _) = await svc.LogAsync(req, _author);

        dto!.OccurredAt.ShouldBe(pastDate);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ListByEntityAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ListByEntityAsync_EntityNotFound_ReturnsError()
    {
        var svc = CreateService();

        var (dto, error) = await svc.ListByEntityAsync(EntityType, Guid.NewGuid(), 1, 25, _author);

        error.ShouldBe("entity_not_found");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task ListByEntityAsync_EmptyList_ReturnsPaginatedEmpty()
    {
        SeedEntity();
        var svc = CreateService();

        var (dto, error) = await svc.ListByEntityAsync(EntityType, _entityId, 1, 25, _author);

        error.ShouldBeNull();
        dto.ShouldNotBeNull();
        dto!.Data.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
        dto.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task ListByEntityAsync_WithEvents_ReturnsPaginatedResult()
    {
        SeedEntity();
        SeedEvent();
        SeedEvent();
        var svc = CreateService();

        var (dto, error) = await svc.ListByEntityAsync(EntityType, _entityId, 1, 25, _author);

        error.ShouldBeNull();
        dto!.TotalCount.ShouldBe(2);
        dto.Data.Count.ShouldBe(2);
        dto.Page.ShouldBe(1);
        dto.PageSize.ShouldBe(25);
    }

    [Fact]
    public async Task ListByEntityAsync_AdminSeesRedactionReason_NonAdminDoesNot()
    {
        SeedEntity();
        var comm = SeedEvent(isRedacted: true);
        comm.RedactionReason = "Sensitive content";
        var svc = CreateService();

        var (adminDto, _) = await svc.ListByEntityAsync(EntityType, _entityId, 1, 25, _admin);
        var (userDto, _) = await svc.ListByEntityAsync(EntityType, _entityId, 1, 25, _otherUser);

        adminDto!.Data[0].RedactionReason.ShouldBe("Sensitive content");
        userDto!.Data[0].RedactionReason.ShouldBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GetByIdAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsError()
    {
        var svc = CreateService();

        var (dto, error) = await svc.GetByIdAsync(Guid.NewGuid(), _author);

        error.ShouldBe("not_found");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsDto()
    {
        var comm = SeedEvent();
        var svc = CreateService();

        var (dto, error) = await svc.GetByIdAsync(comm.Id, _author);

        error.ShouldBeNull();
        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(comm.Id);
        dto.EventType.ShouldBe("Note");
        dto.Body.ShouldBe("Original body");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  EditAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EditAsync_AllNullFields_ReturnsNoChanges()
    {
        var comm = SeedEvent(authorId: _author.UserId);
        var svc = CreateService();
        var req = new CommunicationEditRequest(null, null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _author);

        error.ShouldBe("no_changes");
        dto.ShouldBeNull();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task EditAsync_NotFound_ReturnsError()
    {
        var svc = CreateService();
        var req = new CommunicationEditRequest("New body", null, null, null);

        var (dto, error) = await svc.EditAsync(Guid.NewGuid(), req, _author);

        error.ShouldBe("not_found");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task EditAsync_Redacted_ReturnsEventRedacted()
    {
        var comm = SeedEvent(authorId: _author.UserId, isRedacted: true);
        var svc = CreateService();
        var req = new CommunicationEditRequest("New body", null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _author);

        error.ShouldBe("event_redacted");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task EditAsync_NotAuthor_ReturnsNotAuthor()
    {
        var comm = SeedEvent(authorId: _author.UserId);
        var svc = CreateService();
        var req = new CommunicationEditRequest("New body", null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _otherUser);

        error.ShouldBe("not_author");
        dto.ShouldBeNull();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task EditAsync_EditWindowExpired_ReturnsError()
    {
        var comm = SeedEvent(
            authorId: _author.UserId,
            createdAt: DateTime.UtcNow.AddHours(-25));
        var svc = CreateService();
        var req = new CommunicationEditRequest("New body", null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _author);

        error.ShouldBe("edit_window_expired");
        dto.ShouldBeNull();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task EditAsync_AdminEditsOtherUserEvent_Succeeds()
    {
        var comm = SeedEvent(authorId: _author.UserId);
        var svc = CreateService();
        var req = new CommunicationEditRequest("Admin override", null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _admin);

        error.ShouldBeNull();
        dto!.Body.ShouldBe("Admin override");
        _unitOfWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task EditAsync_AdminIgnoresEditWindow_Succeeds()
    {
        var comm = SeedEvent(
            authorId: _author.UserId,
            createdAt: DateTime.UtcNow.AddDays(-7));
        var svc = CreateService();
        var req = new CommunicationEditRequest("Admin override of old event", null, null, null);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _admin);

        error.ShouldBeNull();
        dto!.Body.ShouldBe("Admin override of old event");
    }

    [Fact]
    public async Task EditAsync_AuthorWithinWindow_UpdatesFields()
    {
        var comm = SeedEvent(authorId: _author.UserId);
        var svc = CreateService();
        var newDate = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var req = new CommunicationEditRequest("Updated body", "Updated subject", "Updated outcome", newDate);

        var (dto, error) = await svc.EditAsync(comm.Id, req, _author);

        error.ShouldBeNull();
        dto!.Body.ShouldBe("Updated body");
        dto.Subject.ShouldBe("Updated subject");
        dto.Outcome.ShouldBe("Updated outcome");
        dto.OccurredAt.ShouldBe(newDate);
        comm.LastEditedAt.ShouldNotBeNull();
        comm.LastEditedByUserId.ShouldBe(_author.UserId);
        _unitOfWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task EditAsync_EmitsTimelineEvent()
    {
        var comm = SeedEvent(authorId: _author.UserId);
        var svc = CreateService();
        var req = new CommunicationEditRequest("Edited body", null, null, null);

        await svc.EditAsync(comm.Id, req, _author);

        _timelineRepo.Events.Count.ShouldBe(1);
        _timelineRepo.Events[0].EventType.ShouldBe("CommunicationEdited");
        _timelineRepo.Events[0].ActorUserId.ShouldBe(_author.UserId);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  RedactAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RedactAsync_NotFound_ReturnsError()
    {
        var svc = CreateService();
        var req = new CommunicationRedactRequest("Test reason");

        var (dto, error) = await svc.RedactAsync(Guid.NewGuid(), req, _admin);

        error.ShouldBe("not_found");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task RedactAsync_AlreadyRedacted_ReturnsError()
    {
        var comm = SeedEvent(isRedacted: true);
        var svc = CreateService();
        var req = new CommunicationRedactRequest("Second attempt");

        var (dto, error) = await svc.RedactAsync(comm.Id, req, _admin);

        error.ShouldBe("already_redacted");
        dto.ShouldBeNull();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task RedactAsync_ClearsBodyAndOutcome()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationRedactRequest("Compliance requirement");

        var (dto, error) = await svc.RedactAsync(comm.Id, req, _admin);

        error.ShouldBeNull();
        comm.Body.ShouldBeNull();
        comm.Outcome.ShouldBeNull();
        dto!.Body.ShouldBeNull();
        dto.Outcome.ShouldBeNull();
    }

    [Fact]
    public async Task RedactAsync_SetsRedactionFields()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationRedactRequest("Compliance requirement");

        var (dto, error) = await svc.RedactAsync(comm.Id, req, _admin);

        error.ShouldBeNull();
        comm.IsRedacted.ShouldBeTrue();
        comm.RedactedAt.ShouldNotBeNull();
        comm.RedactedByUserId.ShouldBe(_admin.UserId);
        comm.RedactionReason.ShouldBe("Compliance requirement");
        dto!.IsRedacted.ShouldBeTrue();
        _unitOfWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task RedactAsync_EmitsTimelineEvent()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationRedactRequest("GDPR");

        await svc.RedactAsync(comm.Id, req, _admin);

        _timelineRepo.Events.Count.ShouldBe(1);
        _timelineRepo.Events[0].EventType.ShouldBe("CommunicationRedacted");
        _timelineRepo.Events[0].ActorUserId.ShouldBe(_admin.UserId);
    }

    [Fact]
    public async Task RedactAsync_AdminRedactionReasonVisibleToAdmin_HiddenFromOthers()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationRedactRequest("Secret compliance reason");

        var (adminDto, _) = await svc.RedactAsync(comm.Id, req, _admin);

        // Re-seed and call GetById as non-admin
        var (userDto, _) = await svc.GetByIdAsync(comm.Id, _otherUser);

        adminDto!.RedactionReason.ShouldBe("Secret compliance reason");
        userDto!.RedactionReason.ShouldBeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CreateFollowUpAsync
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateFollowUpAsync_NotFound_ReturnsError()
    {
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Follow up task", null, null, null);

        var (dto, error) = await svc.CreateFollowUpAsync(Guid.NewGuid(), req, _author);

        error.ShouldBe("not_found");
        dto.ShouldBeNull();
    }

    [Fact]
    public async Task CreateFollowUpAsync_FollowUpExists_ReturnsError()
    {
        var existingTaskId = Guid.NewGuid();
        var comm = SeedEvent(followUpTaskId: existingTaskId);
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Another follow up", null, null, null);

        var (dto, error) = await svc.CreateFollowUpAsync(comm.Id, req, _author);

        error.ShouldBe("follow_up_exists");
        dto.ShouldBeNull();
        _unitOfWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task CreateFollowUpAsync_Success_CreatesTaskItem()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var req = new CommunicationFollowUpRequest("Follow up on call", dueDate, null, "High");

        var (dto, error) = await svc.CreateFollowUpAsync(comm.Id, req, _author);

        error.ShouldBeNull();
        dto.ShouldNotBeNull();
        dto!.Title.ShouldBe("Follow up on call");
        dto.Status.ShouldBe("Open");
        _taskRepo.Added.Count.ShouldBe(1);
        _taskRepo.Added[0].Priority.ShouldBe("High");
        _unitOfWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task CreateFollowUpAsync_SetsFollowUpTaskIdOnEvent()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Follow up", null, null, null);

        var (dto, _) = await svc.CreateFollowUpAsync(comm.Id, req, _author);

        comm.FollowUpTaskId.ShouldBe(dto!.Id);
    }

    [Fact]
    public async Task CreateFollowUpAsync_UsesCurrentUserWhenNoAssignee()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Self-assigned follow up", null, null, null);

        await svc.CreateFollowUpAsync(comm.Id, req, _author);

        _taskRepo.Added[0].AssignedToUserId.ShouldBe(_author.UserId);
    }

    [Fact]
    public async Task CreateFollowUpAsync_UsesProvidedAssignee()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var assigneeId = Guid.NewGuid();
        var req = new CommunicationFollowUpRequest("Assigned follow up", null, assigneeId, null);

        await svc.CreateFollowUpAsync(comm.Id, req, _author);

        _taskRepo.Added[0].AssignedToUserId.ShouldBe(assigneeId);
    }

    [Fact]
    public async Task CreateFollowUpAsync_DefaultPriority_WhenNotProvided()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Default priority", null, null, null);

        await svc.CreateFollowUpAsync(comm.Id, req, _author);

        _taskRepo.Added[0].Priority.ShouldBe("Normal");
    }

    [Fact]
    public async Task CreateFollowUpAsync_LinkedEntityPointsToCommunicationEvent()
    {
        var comm = SeedEvent();
        var svc = CreateService();
        var req = new CommunicationFollowUpRequest("Linked task", null, null, null);

        await svc.CreateFollowUpAsync(comm.Id, req, _author);

        _taskRepo.Added[0].LinkedEntityType.ShouldBe("CommunicationEvent");
        _taskRepo.Added[0].LinkedEntityId.ShouldBe(comm.Id);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
//  Stub — CommunicationEventRepository
// ═══════════════════════════════════════════════════════════════════════════

internal class StubCommunicationEventRepository : ICommunicationEventRepository
{
    private readonly Dictionary<Guid, CommunicationEvent> _events = new();
    private readonly HashSet<(string, Guid)> _existingEntities = new();

    public List<CommunicationEvent> Added { get; } = [];

    public void Seed(CommunicationEvent comm) => _events[comm.Id] = comm;

    public void SeedEntity(string entityType, Guid entityId) =>
        _existingEntities.Add((entityType, entityId));

    public Task<CommunicationEvent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_events.GetValueOrDefault(id));

    public Task<(IReadOnlyList<CommunicationEvent> Items, int TotalCount)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default)
    {
        var all = _events.Values
            .Where(e => e.PrimaryEntityType == entityType && e.PrimaryEntityId == entityId)
            .OrderByDescending(e => e.OccurredAt)
            .ToList();
        var total = all.Count;
        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<CommunicationEvent>, int)>((paged, total));
    }

    public Task AddAsync(CommunicationEvent comm, CancellationToken ct = default)
    {
        Added.Add(comm);
        _events[comm.Id] = comm;
        return Task.CompletedTask;
    }

    public Task<bool> EntityExistsAsync(string entityType, Guid entityId, CancellationToken ct = default) =>
        Task.FromResult(_existingEntities.Contains((entityType, entityId)));
}
