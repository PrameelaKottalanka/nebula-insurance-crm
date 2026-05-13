# F0021 — Communication Hub & Activity Capture — Status

**Overall Status:** In Progress
**Last Updated:** 2026-05-07

## Story Checklist

| Story | Title | Status |
|-------|-------|--------|
| F0021-S0001 | Log a communication note | Done |
| F0021-S0002 | Log a phone call | Done |
| F0021-S0003 | Log a meeting | Done |
| F0021-S0004 | View communication feed on entity | Done |
| F0021-S0005 | Create follow-up task from communication | Done |
| F0021-S0006 | Edit and redact a communication event | Done |
| F0021-S0007 | Filter communication feed by type and date | Deferred |

## Backend Progress

- [x] CommunicationEvent entity and enums (EventType, Direction, EntityType)
- [x] EF configuration and migration (AddCommunicationEvents)
- [x] ICommunicationEventRepository + CommunicationEventRepository
- [x] CommunicationEventService (Log, List, GetById, Edit, Redact, CreateFollowUp)
- [x] DTOs (LogRequest, EditRequest, RedactRequest, SummaryDto, DetailDto)
- [x] CommunicationEndpoints registered in Program.cs
- [x] Authorization policies (communication:create/read/edit/redact in policy.csv)
- [ ] Unit tests passing (written; require Docker to run)
- [ ] Integration tests passing (written; require Docker to run)

## Frontend Progress

- [x] CommunicationEvent types and API hooks (useCommunicationEvents, useLogCommunication, useEditCommunication, useRedactCommunication)
- [x] Communications tab added to Account, Broker, Submission, Policy, Renewal detail pages
- [x] CommunicationFeed (month grouping, pagination, empty state)
- [x] CommunicationEventCard (Note, Call, Meeting variants; redacted state; edited label)
- [x] LogCommunicationModal (Note / Call / Meeting tabs)
- [x] CreateFollowUpModal (inline in CommunicationEventCard)
- [ ] Filter bar — S0007 (Phase 1, deferred)
- [ ] Component and integration tests added
- [ ] Accessibility validation recorded
- [ ] Responsive layout verified

## Cross-Cutting

- [ ] Migration applied (requires Docker + Postgres)
- [ ] Seed data (if applicable)
- [x] Bruno API collection updated for communication endpoints (6 requests in bruno/nebula/communications/)
- [x] No TODOs remain in code

## Required Signoff Roles (Set in Planning)

| Role | Required | Why Required | Set By | Date |
|------|----------|--------------|--------|------|
| Quality Engineer | Yes | Activity capture and relationship timeline behavior require validation. | Architect | TBD |
| Code Reviewer | Yes | Entity linkage and activity behavior require independent review. | Architect | TBD |
| Security Reviewer | TBD | Set during refinement if email-linked data or sensitive content boundaries are introduced. | Architect | TBD |
| DevOps | TBD | Set during refinement if integration or processing dependencies are introduced. | Architect | TBD |
| Architect | TBD | Set during refinement if activity-model design decisions require explicit approval. | Architect | TBD |

## Story Signoff Provenance

| Story | Role | Reviewer | Verdict | Evidence | Date | Notes |
|-------|------|----------|---------|----------|------|-------|
| F0021-S0001 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0001 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0002 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0002 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0003 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0003 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0004 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0004 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0005 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0005 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0006 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0006 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
| F0021-S0007 | Quality Engineer | - | N/A | - | - | Populate during implementation. |
| F0021-S0007 | Code Reviewer | - | N/A | - | - | Populate during implementation. |
