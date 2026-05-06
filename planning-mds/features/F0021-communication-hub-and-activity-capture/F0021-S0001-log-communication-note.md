---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0001: Log a Communication Note

**Story ID:** F0021-S0001
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Log a communication note
**Priority:** Critical
**Phase:** CRM Release MVP

## User Story

**As a** distribution user or underwriter
**I want** to log a free-text note against a broker, account, submission, policy, or renewal
**So that** the interaction is captured in the CRM record and visible to anyone working the relationship

## Context & Background

Notes are the simplest and most common form of communication capture. A user records a quick summary of a conversation, decision, or observation — linked directly to the relevant entity. Notes appear in the Communications feed on the entity detail page, sorted newest first. This story establishes the foundational CommunicationEvent model and log flow that calls (S0002) and meetings (S0003) extend.

## Acceptance Criteria

**Happy Path:**
- **Given** a distribution user or underwriter with `communication:create` permission
- **When** they submit a log request with Body, OccurredAt, PrimaryEntityType, and PrimaryEntityId
- **Then** a CommunicationEvent of type `Note` is created, AuthoredByUserId and AuthoredByDisplayName are set from the authenticated principal, an ActivityTimelineEvent (`CommunicationLogged`) is appended to the linked entity, and the event ID is returned

**Default OccurredAt:**
- **Given** the user does not supply OccurredAt
- **When** they submit the request
- **Then** OccurredAt defaults to the server-side current timestamp

**Alternative Flows / Edge Cases:**
- Body missing or empty → HTTP 400 with `ProblemDetails`, `code=body_required`
- Body exceeds 4000 characters → HTTP 400 with `code=body_too_long`
- PrimaryEntityType not in known set → HTTP 400 with `code=invalid_entity_type`
- PrimaryEntityId not found or soft-deleted → HTTP 404 with `code=entity_not_found`
- OccurredAt in the future → HTTP 400 with `code=occurred_at_future`
- User lacks `communication:create` permission → HTTP 403
- BrokerUser role → HTTP 403 (notes are internal-only)

**Checklist:**
- [ ] Note created with EventType = `Note`
- [ ] AuthoredByUserId and AuthoredByDisplayName set from JWT principal
- [ ] OccurredAt defaults to now when not supplied
- [ ] OccurredAt in the future is rejected
- [ ] ActivityTimelineEvent (`CommunicationLogged`) appended atomically with note creation
- [ ] BrokerDescription on ActivityTimelineEvent is null (communication events are internal-only in MVP)
- [ ] Created note appears at the top of the Communications feed for the linked entity (newest first)
- [ ] All validation errors use RFC 7807 ProblemDetails format

## Data Requirements

**Required Fields:**
- `body` (string, max 4000 chars): Free-text content of the note
- `primaryEntityType` (enum): One of `Broker | Account | Submission | Policy | Renewal`
- `primaryEntityId` (uuid): ID of the linked entity

**Optional Fields:**
- `occurredAt` (datetime): When the interaction happened; defaults to now
- `subject` (string, max 200 chars): Short title for the note

**Validation Rules:**
- Body is required and must be 1–4000 characters
- PrimaryEntityType must be one of the five known values
- PrimaryEntityId must resolve to a non-deleted entity of the specified type
- OccurredAt, when provided, must not be in the future
- Subject, when provided, must be 1–200 characters

## Role-Based Visibility

**Roles that can create:**
- Distribution User — Full create access
- Underwriter — Full create access
- Distribution Manager — Full create access
- Admin — Full create access

**Roles that cannot create:**
- BrokerUser — Notes are internal-only; HTTP 403 returned

**Data Visibility:**
- InternalOnly: All communication notes are internal in MVP
- ExternalVisible: None (BrokerDescription on ActivityTimelineEvent is null)

## Non-Functional Expectations

- Performance: Note creation completes in < 500ms
- Security: ABAC enforced; entity access check runs before note creation (user must have read access to the linked entity)
- Reliability: Note creation and ActivityTimelineEvent append are atomic (single DB transaction)

## Dependencies

**Depends On:**
- F0002 — Broker entity (archived)
- F0016 — Account entity (archived)
- F0006 — Submission entity (archived)
- F0018 — Policy entity (archived)
- F0007 — Renewal entity (archived)

**Related Stories:**
- F0021-S0002 — Extends this model for phone calls
- F0021-S0003 — Extends this model for meetings
- F0021-S0004 — Displays notes in the Communications feed

## Business Rules

1. **Internal-Only in MVP:** All communication notes are internal. BrokerUsers receive HTTP 403 on create. BrokerDescription on the corresponding ActivityTimelineEvent is null so broker-scoped timeline queries do not surface the event.
2. **Entity Access Required:** The authenticated user must have read access to the linked entity (enforced via existing ABAC policies). A user who cannot read an account cannot log a note against it.
3. **OccurredAt Cannot Be Future:** Communication events record past interactions. Future timestamps are rejected to prevent incorrect timeline ordering.
4. **Atomic Persistence:** The CommunicationEvent record and its ActivityTimelineEvent are written in a single transaction. If either fails, neither is persisted.
5. **AuthoredBy Snapshot:** AuthoredByDisplayName is snapshotted from the user's display name at creation time so the feed renders correctly even if the user's profile changes later.

## Out of Scope

- Rich-text or markdown formatting in the note body
- File attachments on notes (documents belong to F0020)
- Tagging or categorisation beyond the built-in EventType
- Email-linked notes (deferred to F0030 Integration Hub)
- BrokerUser visibility of notes

## UI/UX Notes

- Screens involved: Log Communication modal (accessed from `[+ Log Communication]` on entity detail page), Communications feed tab
- Key interactions: User opens modal, selects "Note" type (default), enters body, optionally edits OccurredAt and subject, clicks "Log Communication"
- On success → modal closes, new note card appears at top of feed without full page reload

## Questions & Assumptions

**Open Questions:**
- [ ] Should notes support @mentions to notify colleagues? (Deferred — not in MVP scope)

**Assumptions (to be validated):**
- OccurredAt defaults to now; users may backdate freely
- Notes are strictly internal in MVP; BrokerUser visibility deferred to a later feature
- Subject is optional; most users will leave it blank for quick notes

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Edge cases handled (missing body, future OccurredAt, invalid entity, BrokerUser 403)
- [ ] Permissions enforced (ABAC entity access + communication:create)
- [ ] Audit/timeline logged (CommunicationLogged ActivityTimelineEvent appended atomically)
- [ ] Tests pass
- [ ] Documentation updated (if needed)
- [ ] Story filename matches `Story ID` prefix (`F0021-S0001-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
