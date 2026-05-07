# Feature Assembly Plan — F0021: Communication Hub & Activity Capture

**Created:** 2026-05-07
**Author:** Architect Agent
**Status:** Ready for Backend Implementation

> **Scope:** Stories S0001–S0006 only. S0007 (filter bar) is explicitly Phase 1 / post-MVP and is not covered here.

---

## Overview

F0021 introduces a `CommunicationEvent` entity and the full log/list/edit/redact/follow-up lifecycle. No existing entities change shape — this is a net-new domain object with polymorphic linkage to Broker, Account, Submission, Policy, and Renewal records. The feature adds a Communications tab to all five entity detail pages on the frontend and wires follow-up creation into the existing `TaskItem` model without schema changes on the task side.

---

## Build Order

| Step | Scope | Stories | Rationale |
|------|-------|---------|-----------|
| 1 | Domain + Infrastructure | All | Entity, EF config, repository, and migration are the shared foundation all stories depend on |
| 2 | Application + API: Log & List | S0001, S0002, S0003, S0004 | Create and read paths; establishes the service and endpoint skeleton |
| 3 | Application + API: Edit, Redact, Follow-Up | S0005, S0006 | Mutation paths that depend on the entity already existing |
| 4 | Frontend: Feature Slice + Tab Wiring | All | Types, hooks, components, and tab integration across 5 entity pages |

---

## Existing Code (Must Be Modified)

| File | Current State | F0021 Change |
|------|---------------|--------------|
| `engine/src/Nebula.Infrastructure/Persistence/AppDbContext.cs` | 20+ DbSets | **Expand** — add `DbSet<CommunicationEvent> CommunicationEvents` |
| `engine/src/Nebula.Infrastructure/DependencyInjection.cs` | 15+ service/repo registrations | **Expand** — register `ICommunicationEventRepository` → `CommunicationEventRepository` and `CommunicationEventService` |
| `engine/src/Nebula.Api/Program.cs` | 12 endpoint group registrations | **Expand** — add `app.MapCommunicationEndpoints()` |
| `planning-mds/api/nebula-api.yaml` | No Communication paths or schemas | **Expand** — add 6 paths, 4 request schemas, 1 response schema, 1 paginated list schema (see `nebula-api-f0021-patch.yaml`) |

---

## New Files

| File | Layer | Purpose |
|------|-------|---------|
| `engine/src/Nebula.Domain/Entities/CommunicationEvent.cs` | Domain | Entity with full field set |
| `engine/src/Nebula.Domain/Entities/CommunicationEventConstants.cs` | Domain | String constants for EventType, Direction, PrimaryEntityType |
| `engine/src/Nebula.Infrastructure/Persistence/Configurations/CommunicationEventConfiguration.cs` | Infrastructure | EF Core fluent config + indexes |
| `engine/src/Nebula.Infrastructure/Persistence/Repositories/CommunicationEventRepository.cs` | Infrastructure | EF repository implementation |
| `engine/src/Nebula.Infrastructure/Persistence/Migrations/<timestamp>_F0021_AddCommunicationEvents.cs` | Infrastructure | EF migration (generated via dotnet ef) |
| `engine/src/Nebula.Application/Interfaces/ICommunicationEventRepository.cs` | Application | Repository contract |
| `engine/src/Nebula.Application/DTOs/CommunicationDtos.cs` | Application | All request/response DTOs for this feature |
| `engine/src/Nebula.Application/Validators/CommunicationValidators.cs` | Application | FluentValidation validators for log, edit, redact, follow-up |
| `engine/src/Nebula.Application/Services/CommunicationEventService.cs` | Application | Business logic: Log, List, GetById, Edit, Redact, CreateFollowUp |
| `engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs` | API | Minimal API endpoint group |
| `experience/src/features/communications/types.ts` | Frontend | TypeScript types for all DTOs |
| `experience/src/features/communications/hooks/useCommunicationEvents.ts` | Frontend | TanStack Query list hook |
| `experience/src/features/communications/hooks/useLogCommunication.ts` | Frontend | Log mutation hook |
| `experience/src/features/communications/hooks/useEditCommunication.ts` | Frontend | Edit mutation hook |
| `experience/src/features/communications/hooks/useRedactCommunication.ts` | Frontend | Redact mutation hook |
| `experience/src/features/communications/hooks/useCreateFollowUp.ts` | Frontend | Follow-up mutation hook |
| `experience/src/features/communications/components/CommunicationFeed.tsx` | Frontend | Paged list with month grouping + empty state |
| `experience/src/features/communications/components/CommunicationEventCard.tsx` | Frontend | Note / Call / Meeting card variants + redacted state |
| `experience/src/features/communications/components/LogCommunicationModal.tsx` | Frontend | Three-tab log modal |
| `experience/src/features/communications/components/CreateFollowUpModal.tsx` | Frontend | Post-log follow-up task modal |
| `experience/src/features/communications/components/EditCommunicationModal.tsx` | Frontend | Edit modal (pre-filled, 24h window) |
| `experience/src/features/communications/components/RedactCommunicationModal.tsx` | Frontend | Admin redact modal with required reason field |
| `experience/src/features/communications/index.ts` | Frontend | Feature barrel export |
| `planning-mds/schemas/communication-log-request.schema.json` | Shared | JSON Schema validation — log request |
| `planning-mds/schemas/communication-edit-request.schema.json` | Shared | JSON Schema validation — edit request |
| `planning-mds/schemas/communication-redact-request.schema.json` | Shared | JSON Schema validation — redact request |
| `planning-mds/schemas/communication-follow-up-request.schema.json` | Shared | JSON Schema validation — follow-up request |
| `planning-mds/schemas/communication-event-summary.schema.json` | Shared | JSON Schema — response DTO |
| `planning-mds/schemas/paginated-communication-list.schema.json` | Shared | JSON Schema — paginated list response |
| `planning-mds/api/nebula-api-f0021-patch.yaml` | Shared | OpenAPI additions to be merged into nebula-api.yaml |

---

## Step 1 — Domain + Infrastructure Foundation (All Stories)

### New Files

| File | Layer |
|------|-------|
| `engine/src/Nebula.Domain/Entities/CommunicationEvent.cs` | Domain |
| `engine/src/Nebula.Domain/Entities/CommunicationEventConstants.cs` | Domain |
| `engine/src/Nebula.Infrastructure/Persistence/Configurations/CommunicationEventConfiguration.cs` | Infrastructure |
| `engine/src/Nebula.Infrastructure/Persistence/Repositories/CommunicationEventRepository.cs` | Infrastructure |

### Modified Files

| File | Change |
|------|--------|
| `engine/src/Nebula.Infrastructure/Persistence/AppDbContext.cs` | Add `public DbSet<CommunicationEvent> CommunicationEvents => Set<CommunicationEvent>();` |
| `engine/src/Nebula.Infrastructure/DependencyInjection.cs` | Register `ICommunicationEventRepository` → `CommunicationEventRepository` (scoped) |

### Entity Definition

```csharp
// engine/src/Nebula.Domain/Entities/CommunicationEvent.cs
namespace Nebula.Domain.Entities;

public class CommunicationEvent : BaseEntity
{
    // Core event classification
    public string EventType { get; set; } = default!;        // "Note" | "Call" | "Meeting"
    public string? Direction { get; set; }                    // "Inbound" | "Outbound" | "Internal"; null for Note
    public int? DurationMinutes { get; set; }                 // null for Note; 1–600 for Call/Meeting

    // Shared content fields
    public string? Subject { get; set; }                      // max 200; optional on all types
    public string? Body { get; set; }                         // max 4000; null when IsRedacted=true
    public string? Outcome { get; set; }                      // max 1000; Call/Meeting only; optional

    // When the interaction occurred (can be backdated; cannot be future)
    public DateTime OccurredAt { get; set; }

    // Polymorphic entity linkage
    public string PrimaryEntityType { get; set; } = default!; // "Broker"|"Account"|"Submission"|"Policy"|"Renewal"
    public Guid PrimaryEntityId { get; set; }

    // Authorship snapshot — set at log time; stable if UserProfile display name changes later
    public Guid AuthoredByUserId { get; set; }
    public string AuthoredByDisplayName { get; set; } = default!;

    // Edit tracking (null until first edit)
    public DateTime? LastEditedAt { get; set; }
    public Guid? LastEditedByUserId { get; set; }

    // Redaction fields (null until redacted; redaction is irreversible in MVP)
    public bool IsRedacted { get; set; }
    public DateTime? RedactedAt { get; set; }
    public Guid? RedactedByUserId { get; set; }
    public string? RedactedByDisplayName { get; set; }        // snapshot of admin display name
    public string? RedactionReason { get; set; }              // max 500; required on redact

    // Follow-up task linkage (one per event in MVP; set after task creation)
    public Guid? FollowUpTaskId { get; set; }
}
```

### Constants Definition

```csharp
// engine/src/Nebula.Domain/Entities/CommunicationEventConstants.cs
namespace Nebula.Domain.Entities;

public static class CommunicationEventConstants
{
    public static class EventType
    {
        public const string Note = "Note";
        public const string Call = "Call";
        public const string Meeting = "Meeting";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string> { Note, Call, Meeting };
    }

    public static class Direction
    {
        public const string Inbound = "Inbound";
        public const string Outbound = "Outbound";
        public const string Internal = "Internal";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string> { Inbound, Outbound, Internal };

        // Internal is not valid for calls — only for meetings
        public static readonly IReadOnlySet<string> CallDirections =
            new HashSet<string> { Inbound, Outbound };
    }

    public static class PrimaryEntityType
    {
        public const string Broker = "Broker";
        public const string Account = "Account";
        public const string Submission = "Submission";
        public const string Policy = "Policy";
        public const string Renewal = "Renewal";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string> { Broker, Account, Submission, Policy, Renewal };
    }
}
```

### EF Core Configuration

```csharp
// engine/src/Nebula.Infrastructure/Persistence/Configurations/CommunicationEventConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nebula.Domain.Entities;

namespace Nebula.Infrastructure.Persistence.Configurations;

public class CommunicationEventConfiguration : IEntityTypeConfiguration<CommunicationEvent>
{
    public void Configure(EntityTypeBuilder<CommunicationEvent> builder)
    {
        builder.ToTable("CommunicationEvents");
        builder.HasKey(e => e.Id);

        // Core fields
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Direction).HasMaxLength(20);
        builder.Property(e => e.DurationMinutes);
        builder.Property(e => e.Subject).HasMaxLength(200);
        builder.Property(e => e.Body).HasMaxLength(4000);
        builder.Property(e => e.Outcome).HasMaxLength(1000);
        builder.Property(e => e.OccurredAt).IsRequired();

        // Entity linkage
        builder.Property(e => e.PrimaryEntityType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PrimaryEntityId).IsRequired();

        // Authorship snapshot
        builder.Property(e => e.AuthoredByUserId).IsRequired();
        builder.Property(e => e.AuthoredByDisplayName).IsRequired().HasMaxLength(200);

        // Edit tracking
        builder.Property(e => e.LastEditedAt);
        builder.Property(e => e.LastEditedByUserId);

        // Redaction
        builder.Property(e => e.IsRedacted).HasDefaultValue(false);
        builder.Property(e => e.RedactedAt);
        builder.Property(e => e.RedactedByUserId);
        builder.Property(e => e.RedactedByDisplayName).HasMaxLength(200);
        builder.Property(e => e.RedactionReason).HasMaxLength(500);

        // Follow-up linkage
        builder.Property(e => e.FollowUpTaskId);

        // Audit fields from BaseEntity
        builder.Property(e => e.CreatedByUserId).IsRequired();
        builder.Property(e => e.UpdatedByUserId).IsRequired();
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        // Optimistic concurrency — PostgreSQL xmin
        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // No soft-delete query filter — CommunicationEvent uses redaction, not soft delete.
        // IsDeleted remains false permanently and is not used for this entity.

        // Primary list query: entity feed sorted newest-first
        builder.HasIndex(e => new { e.PrimaryEntityType, e.PrimaryEntityId, e.OccurredAt })
            .HasDatabaseName("IX_CommunicationEvents_PrimaryEntity_OccurredAt")
            .IsDescending(false, false, true);

        // Author lookup (for edit window enforcement and "my communications" future use)
        builder.HasIndex(e => e.AuthoredByUserId)
            .HasDatabaseName("IX_CommunicationEvents_AuthoredByUserId");

        // Follow-up task reverse lookup
        builder.HasIndex(e => e.FollowUpTaskId)
            .HasDatabaseName("IX_CommunicationEvents_FollowUpTaskId")
            .HasFilter("\"FollowUpTaskId\" IS NOT NULL");
    }
}
```

### Repository Interface

```csharp
// engine/src/Nebula.Application/Interfaces/ICommunicationEventRepository.cs
namespace Nebula.Application.Interfaces;

public interface ICommunicationEventRepository
{
    Task<CommunicationEvent?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Returns events newest-first, paged. totalCount is the unpaged count.</summary>
    Task<(IReadOnlyList<CommunicationEvent> Items, int TotalCount)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken ct);

    void Add(CommunicationEvent ev);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

### Repository Implementation

```csharp
// engine/src/Nebula.Infrastructure/Persistence/Repositories/CommunicationEventRepository.cs
using Microsoft.EntityFrameworkCore;
using Nebula.Application.Interfaces;
using Nebula.Domain.Entities;

namespace Nebula.Infrastructure.Persistence.Repositories;

public class CommunicationEventRepository(AppDbContext db) : ICommunicationEventRepository
{
    public async Task<CommunicationEvent?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.CommunicationEvents.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<(IReadOnlyList<CommunicationEvent> Items, int TotalCount)> ListByEntityAsync(
        string entityType, Guid entityId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.CommunicationEvents
            .Where(e => e.PrimaryEntityType == entityType && e.PrimaryEntityId == entityId)
            .OrderByDescending(e => e.OccurredAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Add(CommunicationEvent ev) => db.CommunicationEvents.Add(ev);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
```

### Migration Command

```bash
# Run from repo root after Step 1 files are in place and AppDbContext.CommunicationEvents is added:
dotnet ef migrations add F0021_AddCommunicationEvents \
  --project engine/src/Nebula.Infrastructure \
  --startup-project engine/src/Nebula.Api \
  --output-dir Persistence/Migrations
```

> **Manual index note:** EF Core's `IsDescending()` on a composite index requires EF 7+. Verify the generated migration SQL for `IX_CommunicationEvents_PrimaryEntity_OccurredAt` produces `(PrimaryEntityType, PrimaryEntityId, OccurredAt DESC)`. If not, add raw SQL in the migration `Up()`:
> ```sql
> CREATE INDEX "IX_CommunicationEvents_PrimaryEntity_OccurredAt"
>   ON "CommunicationEvents" ("PrimaryEntityType", "PrimaryEntityId", "OccurredAt" DESC);
> ```

### Step 1 Integration Checkpoint

- [ ] `dotnet build` passes with zero warnings
- [ ] `dotnet ef migrations list` shows `F0021_AddCommunicationEvents` in a Pending state
- [ ] `dotnet ef database update` applies the migration without error
- [ ] `CommunicationEvents` table exists in dev PostgreSQL with all expected columns and indexes (verify with `\d "CommunicationEvents"` in psql)

---

## Step 2 — Application + API: Log & List (S0001, S0002, S0003, S0004)

### New Files

| File | Layer |
|------|-------|
| `engine/src/Nebula.Application/DTOs/CommunicationDtos.cs` | Application |
| `engine/src/Nebula.Application/Validators/CommunicationValidators.cs` | Application |
| `engine/src/Nebula.Application/Services/CommunicationEventService.cs` | Application |
| `engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs` | API |

### Modified Files

| File | Change |
|------|--------|
| `engine/src/Nebula.Api/Program.cs` | Add `app.MapCommunicationEndpoints()` after existing endpoint registrations |
| `engine/src/Nebula.Infrastructure/DependencyInjection.cs` | Register `CommunicationEventService` (scoped) |

### DTO Definitions

```csharp
// engine/src/Nebula.Application/DTOs/CommunicationDtos.cs
namespace Nebula.Application.DTOs;

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record LogCommunicationRequest(
    string EventType,           // Required: Note | Call | Meeting
    string? Direction,          // Required for Call/Meeting; null for Note
    int? DurationMinutes,       // Required for Call/Meeting; null for Note; 1–600
    string? Subject,            // Optional; max 200
    string Body,                // Required; max 4000
    string? Outcome,            // Optional; max 1000; Call/Meeting only
    DateTime? OccurredAt,       // Optional; defaults to UtcNow; must not be future
    string PrimaryEntityType,   // Required: Broker | Account | Submission | Policy | Renewal
    Guid PrimaryEntityId        // Required: ID of the linked entity
);

public record EditCommunicationRequest(
    string? Body,               // Optional; if provided: max 4000; must not be empty string
    string? Subject,            // Optional; if provided: max 200
    string? Outcome,            // Optional; if provided: max 1000
    DateTime? OccurredAt        // Optional; if provided: must not be future
);

public record RedactCommunicationRequest(
    string Reason               // Required; max 500
);

public record CreateFollowUpRequest(
    string Title,               // Required; max 500
    DateTime? DueDate,          // Optional
    Guid? AssignedToUserId,     // Optional; defaults to current user
    string? Priority            // Optional; defaults to "Normal"; enum: Low|Normal|High|Urgent
);

// ── Response DTOs ────────────────────────────────────────────────────────────

public record CommunicationEventSummaryDto(
    Guid Id,
    string EventType,
    string? Direction,
    int? DurationMinutes,
    string? Subject,
    string? Body,               // null when IsRedacted=true
    string? Outcome,            // null when IsRedacted=true
    DateTime OccurredAt,
    string PrimaryEntityType,
    Guid PrimaryEntityId,
    Guid AuthoredByUserId,
    string AuthoredByDisplayName,
    DateTime? LastEditedAt,
    bool IsRedacted,
    string? RedactedByDisplayName,
    DateTime? RedactedAt,
    string? RedactionReason,    // shown to Admin only; null in non-Admin responses
    Guid? FollowUpTaskId,
    string? FollowUpTaskTitle,
    string? FollowUpTaskStatus
);
```

### Validator Definitions

```csharp
// engine/src/Nebula.Application/Validators/CommunicationValidators.cs
using FluentValidation;
using Nebula.Application.DTOs;
using Nebula.Domain.Entities;

namespace Nebula.Application.Validators;

public class LogCommunicationRequestValidator : AbstractValidator<LogCommunicationRequest>
{
    public LogCommunicationRequestValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .Must(CommunicationEventConstants.EventType.All.Contains)
            .WithMessage("'EventType' must be Note, Call, or Meeting.")
            .WithErrorCode("invalid_event_type");

        RuleFor(x => x.Body)
            .NotEmpty().WithErrorCode("body_required")
            .MaximumLength(4000).WithErrorCode("body_too_long");

        RuleFor(x => x.Subject)
            .MaximumLength(200).WithErrorCode("subject_too_long")
            .When(x => x.Subject is not null);

        RuleFor(x => x.Outcome)
            .MaximumLength(1000).WithErrorCode("outcome_too_long")
            .When(x => x.Outcome is not null);

        RuleFor(x => x.OccurredAt)
            .Must(dt => dt is null || dt.Value <= DateTime.UtcNow)
            .WithMessage("'OccurredAt' must not be in the future.")
            .WithErrorCode("occurred_at_future");

        RuleFor(x => x.PrimaryEntityType)
            .NotEmpty()
            .Must(CommunicationEventConstants.PrimaryEntityType.All.Contains)
            .WithMessage("'PrimaryEntityType' must be Broker, Account, Submission, Policy, or Renewal.")
            .WithErrorCode("invalid_entity_type");

        RuleFor(x => x.PrimaryEntityId)
            .NotEmpty().WithErrorCode("primary_entity_id_required");

        // Call-specific rules
        When(x => x.EventType == CommunicationEventConstants.EventType.Call, () =>
        {
            RuleFor(x => x.Direction)
                .NotEmpty().WithErrorCode("direction_required")
                .Must(CommunicationEventConstants.Direction.CallDirections.Contains)
                .WithMessage("Direction for a Call must be Inbound or Outbound.")
                .WithErrorCode("invalid_direction");

            RuleFor(x => x.DurationMinutes)
                .NotNull().WithErrorCode("duration_required")
                .GreaterThan(0).WithErrorCode("duration_invalid")
                .LessThanOrEqualTo(600).WithErrorCode("duration_too_long");
        });

        // Meeting-specific rules
        When(x => x.EventType == CommunicationEventConstants.EventType.Meeting, () =>
        {
            RuleFor(x => x.Direction)
                .NotEmpty().WithErrorCode("direction_required")
                .Must(CommunicationEventConstants.Direction.All.Contains)
                .WithMessage("Direction for a Meeting must be Inbound, Outbound, or Internal.")
                .WithErrorCode("invalid_direction");

            RuleFor(x => x.DurationMinutes)
                .NotNull().WithErrorCode("duration_required")
                .GreaterThan(0).WithErrorCode("duration_invalid")
                .LessThanOrEqualTo(600).WithErrorCode("duration_too_long");
        });

        // Note: Direction and DurationMinutes must be absent
        When(x => x.EventType == CommunicationEventConstants.EventType.Note, () =>
        {
            RuleFor(x => x.Direction)
                .Null().WithMessage("'Direction' must not be set for a Note.")
                .WithErrorCode("direction_not_allowed");

            RuleFor(x => x.DurationMinutes)
                .Null().WithMessage("'DurationMinutes' must not be set for a Note.")
                .WithErrorCode("duration_not_allowed");
        });
    }
}

public class EditCommunicationRequestValidator : AbstractValidator<EditCommunicationRequest>
{
    public EditCommunicationRequestValidator()
    {
        // At least one field must be provided
        RuleFor(x => x)
            .Must(x => x.Body is not null || x.Subject is not null ||
                       x.Outcome is not null || x.OccurredAt is not null)
            .WithMessage("At least one field must be provided to edit.")
            .WithErrorCode("no_changes");

        RuleFor(x => x.Body)
            .NotEmpty().WithErrorCode("body_required")
            .MaximumLength(4000).WithErrorCode("body_too_long")
            .When(x => x.Body is not null);

        RuleFor(x => x.Subject)
            .NotEmpty().MaximumLength(200).WithErrorCode("subject_too_long")
            .When(x => x.Subject is not null);

        RuleFor(x => x.Outcome)
            .NotEmpty().MaximumLength(1000).WithErrorCode("outcome_too_long")
            .When(x => x.Outcome is not null);

        RuleFor(x => x.OccurredAt)
            .Must(dt => dt is null || dt.Value <= DateTime.UtcNow)
            .WithMessage("'OccurredAt' must not be in the future.")
            .WithErrorCode("occurred_at_future")
            .When(x => x.OccurredAt is not null);
    }
}

public class RedactCommunicationRequestValidator : AbstractValidator<RedactCommunicationRequest>
{
    public RedactCommunicationRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("reason_required")
            .MaximumLength(500).WithErrorCode("reason_too_long");
    }
}

public class CreateFollowUpRequestValidator : AbstractValidator<CreateFollowUpRequest>
{
    private static readonly IReadOnlySet<string> ValidPriorities =
        new HashSet<string> { "Low", "Normal", "High", "Urgent" };

    public CreateFollowUpRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode("title_required")
            .MaximumLength(500).WithErrorCode("title_too_long");

        RuleFor(x => x.Priority)
            .Must(p => p is null || ValidPriorities.Contains(p))
            .WithMessage("Priority must be Low, Normal, High, or Urgent.")
            .WithErrorCode("invalid_priority")
            .When(x => x.Priority is not null);
    }
}
```

### Service Definition

```csharp
// engine/src/Nebula.Application/Services/CommunicationEventService.cs
// Constructor injection — register as Scoped in DependencyInjection.cs
public class CommunicationEventService(
    ICommunicationEventRepository _repo,
    ITimelineService _timeline,
    ICurrentUserService _currentUser,
    IAuthorizationService _authz,
    AppDbContext _db,                    // used only for EntityExistsAsync — avoids 5 repo deps
    ITaskRepository _taskRepo,           // reused for follow-up task creation
    ILogger<CommunicationEventService> _logger)
```

**Note on AppDbContext direct usage:** The service uses `_db` directly only for `EntityExistsAsync()` — a lightweight existence check across 5 entity types. This avoids injecting 5 entity repositories. The task repository is the only full repository injected beyond the communication repository.

### Logic Flow — LogAsync (S0001, S0002, S0003)

```
LogAsync(LogCommunicationRequest req, NebulaPrincipal principal, CancellationToken ct)
→ returns CommunicationEventSummaryDto
```

1. Authorize `communication:create` via Casbin → 403 `policy_denied` if denied
2. Validate `req.PrimaryEntityType` is in `CommunicationEventConstants.PrimaryEntityType.All` → 400 `invalid_entity_type`
3. Call `EntityExistsAsync(req.PrimaryEntityType, req.PrimaryEntityId, ct)` → 404 `entity_not_found` if false
4. Authorize user has read access to the linked entity: call `_authz.AuthorizeAsync(principal, req.PrimaryEntityType.ToLower(), "read")` → 403 `entity_access_denied` if denied
5. Run FluentValidation `LogCommunicationRequestValidator` → 400 `validation_error` with field-level errors if any rule fails
6. Build entity:
   ```csharp
   var ev = new CommunicationEvent
   {
       EventType         = req.EventType,
       Direction         = req.Direction,
       DurationMinutes   = req.DurationMinutes,
       Subject           = req.Subject,
       Body              = req.Body,
       Outcome           = req.Outcome,
       OccurredAt        = req.OccurredAt ?? DateTime.UtcNow,
       PrimaryEntityType = req.PrimaryEntityType,
       PrimaryEntityId   = req.PrimaryEntityId,
       AuthoredByUserId  = principal.UserId,
       AuthoredByDisplayName = principal.DisplayName,
       CreatedAt         = DateTime.UtcNow,
       CreatedByUserId   = principal.UserId,
       UpdatedAt         = DateTime.UtcNow,
       UpdatedByUserId   = principal.UserId,
   };
   ```
7. `_repo.Add(ev)`
8. Append `ActivityTimelineEvent`:
   ```csharp
   await _timeline.AppendAsync(new ActivityTimelineEvent
   {
       EntityType        = ev.PrimaryEntityType,
       EntityId          = ev.PrimaryEntityId,
       EventType         = "CommunicationLogged",
       EventDescription  = $"{ev.EventType} logged by {principal.DisplayName}",
       BrokerDescription = null,   // always null in MVP — internal-only
       ActorUserId       = principal.UserId,
       ActorDisplayName  = principal.DisplayName,
       OccurredAt        = ev.OccurredAt,
       EventPayloadJson  = JsonSerializer.Serialize(new { communicationEventId = ev.Id, eventType = ev.EventType })
   }, ct);
   ```
9. `await _repo.SaveChangesAsync(ct)` — both the event and timeline event commit atomically (same DbContext transaction)
10. Return `MapToSummaryDto(ev, followUpTask: null)`

### Logic Flow — ListAsync (S0004)

```
ListAsync(string entityType, Guid entityId, int page, int pageSize, NebulaPrincipal principal, CancellationToken ct)
→ returns PaginatedResult<CommunicationEventSummaryDto>
```

1. Validate `entityType` is in `PrimaryEntityType.All` → 400 `invalid_entity_type`
2. Validate `page >= 1`, `pageSize` in `[1, 100]` → 400 `invalid_pagination`
3. Authorize user has `{entityType.ToLower()}:read` via Casbin → 403 if denied
4. Check entity exists → 404 `entity_not_found` if missing
5. `var (items, total) = await _repo.ListByEntityAsync(entityType, entityId, page, pageSize, ct)`
6. Load any `TaskItem` records for events with non-null `FollowUpTaskId` in a single batch query
7. Map each item: `MapToSummaryDto(ev, taskLookup.GetValueOrDefault(ev.FollowUpTaskId))`
8. Return `new PaginatedResult<>(items, page, pageSize, total)`

**Note:** Step 6 loads follow-up tasks in a single query: `await _db.Tasks.Where(t => followUpTaskIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct)`.

### Casbin Enforcement — Log & List

| Endpoint | Resource | Action | Notes |
|----------|----------|--------|-------|
| `POST /communications` | `communication` | `create` | Checked first; then entity-level read check |
| `GET /communications` | `communication` | `read` | Then entity-level read check |
| `GET /communications/{id}` | `communication` | `read` | Then entity-level read check |

BrokerUser role: `communication:create` and `communication:read` both deny → 403 before any business logic.

### Timeline Events

| Operation | EventType | EntityType/Id | EventDescription | BrokerDescription |
|-----------|-----------|---------------|-----------------|-------------------|
| Log Note | `CommunicationLogged` | PrimaryEntityType / PrimaryEntityId | `"Note logged by {DisplayName}"` | `null` |
| Log Call | `CommunicationLogged` | PrimaryEntityType / PrimaryEntityId | `"Call logged by {DisplayName}"` | `null` |
| Log Meeting | `CommunicationLogged` | PrimaryEntityType / PrimaryEntityId | `"Meeting logged by {DisplayName}"` | `null` |

### HTTP Responses — POST /communications

| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `CommunicationEventSummaryDto` | Success |
| 400 | ProblemDetails (`validation_error`) | Validator fails (missing body, invalid direction, etc.) |
| 400 | ProblemDetails (`invalid_entity_type`) | PrimaryEntityType not in known set |
| 403 | ProblemDetails (`policy_denied`) | BrokerUser or missing `communication:create` |
| 403 | ProblemDetails (`entity_access_denied`) | User cannot read the linked entity |
| 404 | ProblemDetails (`entity_not_found`) | PrimaryEntityId does not resolve |

### HTTP Responses — GET /communications

| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | `PaginatedResult<CommunicationEventSummaryDto>` | Success (may be empty array) |
| 400 | ProblemDetails (`invalid_entity_type`) | entityType not in known set |
| 400 | ProblemDetails (`invalid_pagination`) | page < 1 or pageSize out of range |
| 403 | ProblemDetails (`policy_denied`) | BrokerUser or no entity read access |
| 404 | ProblemDetails (`entity_not_found`) | entityId does not resolve |

### CommunicationEndpoints.cs — Skeleton

```csharp
// engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs
namespace Nebula.Api.Endpoints;

public static class CommunicationEndpoints
{
    public static IEndpointRouteBuilder MapCommunicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/communications")
            .WithTags("Communications")
            .RequireAuthorization();

        group.MapPost("/", LogCommunication);
        group.MapGet("/", ListCommunications);
        group.MapGet("/{id:guid}", GetCommunication);
        group.MapPut("/{id:guid}", EditCommunication);
        group.MapPost("/{id:guid}/redact", RedactCommunication);
        group.MapPost("/{id:guid}/follow-up", CreateFollowUp);

        return app;
    }

    // Handler signatures (implementations in Step 2 and Step 3):
    private static async Task<IResult> LogCommunication(
        LogCommunicationRequest req, CommunicationEventService svc,
        ICurrentUserService user, CancellationToken ct) { ... }

    private static async Task<IResult> ListCommunications(
        string entityType, Guid entityId, int? page, int? pageSize,
        CommunicationEventService svc, ICurrentUserService user, CancellationToken ct) { ... }

    private static async Task<IResult> GetCommunication(
        Guid id, CommunicationEventService svc, ICurrentUserService user, CancellationToken ct) { ... }

    // Step 3: EditCommunication, RedactCommunication, CreateFollowUp
}
```

### Step 2 Integration Checkpoint

- [ ] `dotnet build` passes
- [ ] `POST /communications` with a Note body returns 201 with event ID
- [ ] `POST /communications` with EventType=Call, no Direction returns 400 `direction_required`
- [ ] `POST /communications` with BrokerUser JWT returns 403
- [ ] `POST /communications` with unknown PrimaryEntityType returns 400 `invalid_entity_type`
- [ ] `POST /communications` with future OccurredAt returns 400 `occurred_at_future`
- [ ] `GET /communications?entityType=Account&entityId={id}&page=1&pageSize=25` returns 200 with correct shape
- [ ] `ActivityTimelineEvent` with EventType=`CommunicationLogged` present in timeline after log

---

## Step 3 — Application + API: Edit, Redact, Follow-Up (S0005, S0006)

### Logic Flow — EditAsync (S0006)

```
EditAsync(Guid id, EditCommunicationRequest req, NebulaPrincipal principal, CancellationToken ct)
→ returns CommunicationEventSummaryDto
```

1. Authorize `communication:edit` via Casbin → 403 `policy_denied` if denied
2. `var ev = await _repo.GetByIdAsync(id, ct)` → 404 `not_found` if null
3. Guard: `ev.IsRedacted` → 409 `event_redacted`
4. Author/Admin window check:
   - If principal is NOT Admin:
     - If `ev.AuthoredByUserId != principal.UserId` → 403 `not_author`
     - If `DateTime.UtcNow - ev.CreatedAt > TimeSpan.FromHours(24)` → 403 `edit_window_expired`
5. Run `EditCommunicationRequestValidator` → 400 `validation_error` on failure
6. Apply field updates (only non-null fields in the request):
   ```csharp
   if (req.Body is not null)      ev.Body = req.Body;
   if (req.Subject is not null)   ev.Subject = req.Subject;
   if (req.Outcome is not null)   ev.Outcome = req.Outcome;
   if (req.OccurredAt is not null) ev.OccurredAt = req.OccurredAt.Value;
   ev.LastEditedAt       = DateTime.UtcNow;
   ev.LastEditedByUserId = principal.UserId;
   ev.UpdatedAt          = DateTime.UtcNow;
   ev.UpdatedByUserId    = principal.UserId;
   ```
7. Append `ActivityTimelineEvent` (EventType=`CommunicationEdited`, BrokerDescription=null)
8. `await _repo.SaveChangesAsync(ct)`
9. Return `MapToSummaryDto(ev, ...)`

### Logic Flow — RedactAsync (S0006)

```
RedactAsync(Guid id, RedactCommunicationRequest req, NebulaPrincipal principal, CancellationToken ct)
→ returns CommunicationEventSummaryDto
```

1. Authorize `communication:redact` via Casbin → 403 `policy_denied` (only Admin role passes this action)
2. `var ev = await _repo.GetByIdAsync(id, ct)` → 404 `not_found` if null
3. Guard: `ev.IsRedacted` → 409 `already_redacted`
4. Run `RedactCommunicationRequestValidator` → 400 `reason_required`
5. Apply redaction:
   ```csharp
   ev.IsRedacted          = true;
   ev.Body                = null;
   ev.RedactedAt          = DateTime.UtcNow;
   ev.RedactedByUserId    = principal.UserId;
   ev.RedactedByDisplayName = principal.DisplayName;
   ev.RedactionReason     = req.Reason;
   ev.UpdatedAt           = DateTime.UtcNow;
   ev.UpdatedByUserId     = principal.UserId;
   ```
6. Append `ActivityTimelineEvent` (EventType=`CommunicationRedacted`, EventDescription=`$"Communication redacted by {principal.DisplayName}"`, BrokerDescription=null)
7. `await _repo.SaveChangesAsync(ct)`
8. Return `MapToSummaryDto(ev, ...)`

### Logic Flow — CreateFollowUpAsync (S0005)

```
CreateFollowUpAsync(Guid id, CreateFollowUpRequest req, NebulaPrincipal principal, CancellationToken ct)
→ returns TaskSummaryDto
```

1. Authorize `communication:create` via Casbin (reuse create action — user must be able to log communications) → 403
2. `var ev = await _repo.GetByIdAsync(id, ct)` → 404 `not_found` if null
3. Guard: `ev.FollowUpTaskId is not null` → 409 `follow_up_exists`
4. Run `CreateFollowUpRequestValidator` → 400 on failure
5. Create `TaskItem`:
   ```csharp
   var task = new TaskItem
   {
       Title             = req.Title,
       DueDate           = req.DueDate,
       AssignedToUserId  = req.AssignedToUserId ?? principal.UserId,
       Priority          = req.Priority ?? "Normal",
       Status            = "Open",
       LinkedEntityType  = "CommunicationEvent",
       LinkedEntityId    = ev.Id,
       CreatedAt         = DateTime.UtcNow,
       CreatedByUserId   = principal.UserId,
       UpdatedAt         = DateTime.UtcNow,
       UpdatedByUserId   = principal.UserId,
   };
   _taskRepo.Add(task);
   ```
6. Link task back to communication event: `ev.FollowUpTaskId = task.Id; ev.UpdatedAt = DateTime.UtcNow;`
7. `await _repo.SaveChangesAsync(ct)` — commits both the new task and the updated event in one transaction
8. Return `TaskSummaryDto` mapping from `task`

**Note:** Follow-up creation is intentionally NOT atomic with the communication log (per S0005 Business Rule #1). The communication is committed first in Step 2's `LogAsync`. The follow-up is a separate request. If follow-up creation fails, the communication remains saved.

### Casbin Enforcement — Edit, Redact, Follow-Up

| Endpoint | Resource | Action | Notes |
|----------|----------|--------|-------|
| `PUT /communications/{id}` | `communication` | `edit` | All internal roles pass; service enforces author+window |
| `POST /communications/{id}/redact` | `communication` | `redact` | Admin only in Casbin policy |
| `POST /communications/{id}/follow-up` | `communication` | `create` | Reuses create permission |

### HTTP Responses — PUT /communications/{id}

| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | `CommunicationEventSummaryDto` | Success |
| 400 | ProblemDetails (`validation_error`) | Validator fails |
| 400 | ProblemDetails (`no_changes`) | No field provided in request |
| 403 | ProblemDetails (`policy_denied`) | Casbin deny |
| 403 | ProblemDetails (`not_author`) | Non-Admin, non-author user |
| 403 | ProblemDetails (`edit_window_expired`) | Non-Admin author past 24h window |
| 404 | ProblemDetails (`not_found`) | Event ID not found |
| 409 | ProblemDetails (`event_redacted`) | Cannot edit a redacted event |

### HTTP Responses — POST /communications/{id}/redact

| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | `CommunicationEventSummaryDto` | Success |
| 400 | ProblemDetails (`reason_required`) | Reason missing or empty |
| 403 | ProblemDetails (`policy_denied`) | Non-Admin role |
| 404 | ProblemDetails (`not_found`) | Event ID not found |
| 409 | ProblemDetails (`already_redacted`) | Event already redacted |

### HTTP Responses — POST /communications/{id}/follow-up

| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `TaskSummaryDto` | Success |
| 400 | ProblemDetails (`validation_error`) | Title missing or invalid priority |
| 403 | ProblemDetails (`policy_denied`) | Casbin deny |
| 404 | ProblemDetails (`not_found`) | Communication event not found |
| 409 | ProblemDetails (`follow_up_exists`) | FollowUpTaskId already set |

### Timeline Events — Edit & Redact

| Operation | EventType | EventDescription | BrokerDescription |
|-----------|-----------|-----------------|-------------------|
| Edit | `CommunicationEdited` | `"Communication edited by {DisplayName}"` | `null` |
| Redact | `CommunicationRedacted` | `"Communication redacted by {DisplayName}"` | `null` |

### Step 3 Integration Checkpoint

- [ ] `PUT /communications/{id}` by author within 24h returns 200 with `LastEditedAt` set
- [ ] `PUT /communications/{id}` by author after 24h returns 403 `edit_window_expired`
- [ ] `PUT /communications/{id}` by non-author non-Admin returns 403 `not_author`
- [ ] `PUT /communications/{id}` on a redacted event returns 409 `event_redacted`
- [ ] `PUT /communications/{id}` by Admin ignores 24h window
- [ ] `POST /communications/{id}/redact` by Admin returns 200; event.Body is null; IsRedacted=true
- [ ] `POST /communications/{id}/redact` by non-Admin returns 403
- [ ] `POST /communications/{id}/redact` on already-redacted event returns 409
- [ ] `POST /communications/{id}/follow-up` creates a TaskItem and sets FollowUpTaskId on event
- [ ] `POST /communications/{id}/follow-up` second call returns 409 `follow_up_exists`
- [ ] All ActivityTimelineEvents (CommunicationLogged/Edited/Redacted) appear on linked entity timeline

---

## Step 4 — Frontend: Feature Slice + Tab Wiring (All Stories)

### New Files

All in `experience/src/features/communications/`:

| File | Purpose |
|------|---------|
| `types.ts` | TypeScript interfaces matching DTOs |
| `hooks/useCommunicationEvents.ts` | `useQuery` — paged list, keyed by `[entityType, entityId, page]` |
| `hooks/useLogCommunication.ts` | `useMutation` — POST; invalidates list on success |
| `hooks/useEditCommunication.ts` | `useMutation` — PUT; invalidates list + detail on success |
| `hooks/useRedactCommunication.ts` | `useMutation` — POST redact; invalidates list on success |
| `hooks/useCreateFollowUp.ts` | `useMutation` — POST follow-up; invalidates list on success |
| `components/CommunicationFeed.tsx` | Month-grouped feed with "Load earlier" pagination |
| `components/CommunicationEventCard.tsx` | Note/Call/Meeting/Redacted variants |
| `components/LogCommunicationModal.tsx` | Tab-based log modal |
| `components/CreateFollowUpModal.tsx` | Post-log follow-up modal |
| `components/EditCommunicationModal.tsx` | Pre-filled edit modal with 24h client-side hint |
| `components/RedactCommunicationModal.tsx` | Admin-only redact modal |
| `index.ts` | Barrel export |

### Modified Files

| File | Change |
|------|--------|
| Account detail page | Add "Communications" tab rendering `<CommunicationFeed entityType="Account" entityId={id} />` |
| Broker detail page | Add "Communications" tab rendering `<CommunicationFeed entityType="Broker" entityId={id} />` |
| Submission detail page | Add "Communications" tab |
| Policy detail page | Add "Communications" tab |
| Renewal detail page | Add "Communications" tab |
| MSW handlers file | Add handlers for all 6 communication endpoints |

### Key Frontend Constraints

- **Semantic theming:** Use `text-text-primary`, `bg-surface-card`, `border-surface-border` — no raw `zinc/slate/gray` palette classes
- **BrokerUser exclusion:** Do not render the Communications tab if the user's role is `BrokerUser`. Check role from JWT claims
- **Month grouping:** Client-side, computed from `occurredAt` on the returned items — do not request from server
- **Edit button visibility:** Show "Edit" button only when `authoredByUserId === currentUser.id` AND `isRedacted === false` AND `Date.now() - new Date(ev.createdAt) < 86400000`. Admins always see Edit
- **Redact menu:** Show "Redact" in the `···` overflow menu only for Admin role users
- **Follow-up link:** Render `↳ Follow-up: [{title} →]` when `followUpTaskId` is set; use `text-text-muted` with strike-through when `followUpTaskStatus === "Done"`
- **Redacted card:** Render `[Redacted by {redactedByDisplayName} on {redactedAt}]`; hide `Outcome`; use `opacity-60` visual muting

### Step 4 Integration Checkpoint

- [ ] Communications tab present and functional on all five entity detail pages
- [ ] Log Note → card appears at top of feed without page reload
- [ ] Log Call → card shows phone icon, direction badge, duration
- [ ] Log Meeting → card shows meeting icon, direction badge (including "Internal"), duration
- [ ] "Create follow-up" checkbox → follow-up modal → task linked → card shows follow-up link
- [ ] Edit within 24h → "Edited {timestamp}" label appears on card
- [ ] Admin redact → body replaced with placeholder; Outcome hidden; card muted
- [ ] Empty state shown when no communications exist
- [ ] "Load earlier" appends next 25 without clearing existing cards
- [ ] BrokerUser: Communications tab is not rendered
- [ ] `pnpm --dir experience lint` passes
- [ ] `pnpm --dir experience lint:theme` passes (no raw palette classes)
- [ ] `pnpm --dir experience build` passes
- [ ] Component tests for CommunicationFeed, CommunicationEventCard, and LogCommunicationModal pass

---

## Scope Breakdown

| Layer | Required Work | Owner | Status |
|-------|---------------|-------|--------|
| Backend (`engine/`) | Entity, config, repo, migration, DTOs, validators, service (6 methods), endpoints (6 routes) | Backend Developer | Not Started |
| Frontend (`experience/`) | Feature slice (6 components, 5 hooks, types), tab wiring on 5 pages, MSW mocks | Frontend Developer | Not Started |
| Quality | Integration tests for all 6 endpoints; component tests for feed, card, log modal | Quality Engineer | Not Started |
| DevOps/Runtime | None — uses existing PostgreSQL; no new env vars | — | N/A |

---

## Dependency Order

```
Step 0 (Architect):   This plan + JSON schemas + API contract patch — DONE
Step 1 (Backend):     Domain entity + EF config + repository + migration
  ── Checkpoint: migration applies cleanly; table + indexes verified ──
Step 2 (Backend):     DTOs + validators + CommunicationEventService (Log/List) + Endpoints (POST/GET)
  ── Checkpoint: POST/GET /communications functional end-to-end ──
Step 3 (Backend):     Edit/Redact/FollowUp service methods + endpoint handlers
  ── Checkpoint: all 6 endpoints functional; all error codes correct ──
Step 4 (Frontend):    Feature slice + tab wiring + MSW mocks + component tests
  ── Checkpoint: all five tabs functional; BrokerUser exclusion verified ──
Step 5 (QE):          Integration tests + accessibility validation
```

---

## Integration Checklist

- [ ] API contract compatibility: all endpoints implemented exactly per `nebula-api-f0021-patch.yaml`
- [ ] JSON Schema validation shared between frontend (AJV) and backend (FluentValidation rules match schema constraints)
- [ ] `ActivityTimelineEvent` appended atomically on Log, Edit, and Redact (same SaveChanges call)
- [ ] `BrokerDescription` is `null` on all CommunicationEvent-related timeline events (MVP constraint)
- [ ] BrokerUser receives 403 on all communication endpoints
- [ ] 24h edit window enforced server-side (client hint only; not authoritative)
- [ ] Redaction is irreversible — no un-redact endpoint exists
- [ ] Follow-up task committed separately from communication log (not atomic)
- [ ] All error codes match the tables in each Step section
- [ ] `pnpm --dir experience lint:theme` passes (semantic token usage only)
- [ ] Framework vs solution boundary: no `agents/**` changes required for this feature

---

## Risks and Blockers

| Item | Severity | Mitigation | Owner |
|------|----------|------------|-------|
| 5-entity existence check in service uses AppDbContext directly | Low | Acceptable in MVP; encapsulated in private helper; extract to `IEntityExistenceChecker` if pattern recurs | Backend Developer |
| `IsDescending()` on composite EF index requires EF 7+ | Low | Verify generated migration SQL; fall back to raw SQL in migration `Up()` if needed | Backend Developer |
| Casbin `communication:redact` policy not yet in `policy.csv` | Medium | Add `p, Admin, communication, redact` to policy.csv and copy to the embedded resource location | Backend Developer |
| Frontend edit window hint can drift from server if clocks skew | Low | Edit window is enforced server-side; client hint is UI-only (hides button after 24h); mismatch is handled gracefully by 403 `edit_window_expired` | Frontend Developer |

---

## JSON Serialization Convention

- All `DateTime` fields serialized as ISO 8601 UTC strings (`yyyy-MM-ddTHH:mm:ssZ`)
- `Guid` fields serialized as lowercase hyphenated UUIDs (`xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)
- Null optional fields are included in responses with `null` value (not omitted) — consistent with existing API behavior
- camelCase property naming via `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` (already configured globally)

---

## DI Registration Changes

In `engine/src/Nebula.Infrastructure/DependencyInjection.cs`, add:

```csharp
services.AddScoped<ICommunicationEventRepository, CommunicationEventRepository>();
services.AddScoped<CommunicationEventService>();
```

In `engine/src/Nebula.Api/Program.cs`, add after existing `Map*Endpoints()` calls:

```csharp
app.MapCommunicationEndpoints();
```

---

## Casbin Policy Sync

Add the following rows to `policy.csv` (and copy to the embedded resource location in `Nebula.Infrastructure`):

```csv
p, DistributionUser, communication, create
p, DistributionUser, communication, read
p, DistributionUser, communication, edit
p, Underwriter, communication, create
p, Underwriter, communication, read
p, Underwriter, communication, edit
p, DistributionManager, communication, create
p, DistributionManager, communication, read
p, DistributionManager, communication, edit
p, Admin, communication, create
p, Admin, communication, read
p, Admin, communication, edit
p, Admin, communication, redact
# BrokerUser: intentionally absent — all communication actions denied
```

> **Reminder:** After editing `policy.csv`, copy it to the embedded resource path that the `CasbinAuthorizationService` loads at startup. Verify the path in `DependencyInjection.cs` or `appsettings.json`.
