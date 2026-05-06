# F0021 — Communication Hub & Activity Capture — Getting Started

## Prerequisites

- [ ] Read the current release framing in [COMMERCIAL-PC-CRM-RELEASE-PLAN.md](../COMMERCIAL-PC-CRM-RELEASE-PLAN.md)
- [ ] Confirm F0016 (Account 360) is archived — required for account-level communication linkage
- [ ] Confirm F0004 (Task Center UI) is archived — required for follow-up task creation (S0005)
- [ ] Review existing `ActivityTimelineEvent` and `TaskItem` entities in `Nebula.Domain/Entities/`
- [ ] Review the Submission, Policy, and Renewal endpoint patterns in `Nebula.Api/Endpoints/` — CommunicationEndpoints follows the same nested-resource convention

## Services to Run

```bash
# Start infrastructure
docker compose up -d db authentik

# Run backend
dotnet run --project engine/src/Nebula.Api

# Run frontend
pnpm --dir experience install
pnpm --dir experience dev
```

## Key Files

| Layer | Path | Purpose |
|-------|------|---------|
| Entity | `engine/src/Nebula.Domain/Entities/CommunicationEvent.cs` | New entity (to create) |
| Enums | `engine/src/Nebula.Domain/Enums/CommunicationEventType.cs` | Note, Call, Meeting (to create) |
| EF Config | `engine/src/Nebula.Infrastructure/Persistence/Configurations/` | EF fluent config (to create) |
| Repository | `engine/src/Nebula.Infrastructure/Persistence/Repositories/CommunicationEventRepository.cs` | (to create) |
| Service | `engine/src/Nebula.Application/Services/CommunicationEventService.cs` | (to create) |
| Endpoints | `engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs` | (to create) |
| Frontend feature | `experience/src/features/communications/` | Components, hooks, types (to create) |
| Reference pattern | `engine/src/Nebula.Domain/Entities/ActivityTimelineEvent.cs` | Append-only audit pattern |
| Reference pattern | `engine/src/Nebula.Api/Endpoints/SubmissionEndpoints.cs` | Nested-resource routing pattern |

## Environment Variables

No new environment variables required. CommunicationEvent uses the existing PostgreSQL connection.

## How to Verify

1. Start the stack (`docker compose up -d db authentik`, then API + frontend).
2. Navigate to an Account detail page → click "Communications" tab → confirm empty state is shown.
3. Click `[+ Log Communication]` → log a Note → confirm the card appears at the top of the feed.
4. Log a Call (Inbound, 15 min) → confirm phone icon and direction badge on the card.
5. Log a Meeting (Internal, 60 min) → confirm meeting icon and "Internal" badge.
6. Check "Create follow-up task" when logging → confirm follow-up modal appears → create a task → confirm link on the card.
7. Edit a note within 24h → confirm "Edited {timestamp}" label appears on the card.
8. As Admin, redact a note → confirm body is replaced with placeholder; Outcome hidden.
9. Navigate to Broker, Submission, Policy, Renewal detail pages → confirm Communications tab is present and functional on all five.

## Notes

- `CommunicationEvent` is **not** append-only in the same sense as `ActivityTimelineEvent` — it supports controlled edits and redaction. However, the `ActivityTimelineEvent` appended on each log/edit/redact action IS append-only.
- The polymorphic `PrimaryEntityType` + `PrimaryEntityId` pattern on `CommunicationEvent` mirrors the `LinkedEntityType` + `LinkedEntityId` pattern on `TaskItem` from F0004.
- S0007 (filter bar) is Phase 1, not CRM Release MVP. Implement S0001–S0006 first.
- BrokerUser role: Communications tab is not shown in MVP. All events have `BrokerDescription = null`. Do not add BrokerUser routing for this tab.
