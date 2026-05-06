---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0003: Log a Meeting

**Story ID:** F0021-S0003
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Log a meeting
**Priority:** High
**Phase:** CRM Release MVP

## User Story

**As a** distribution user or underwriter
**I want** to log a meeting with duration, direction, and outcome against a CRM entity
**So that** relationship touchpoints — whether in-person, virtual, or internal — are part of the institutional record

## Context & Background

Meetings are a key relationship signal in commercial P&C broker management. A quarterly business review with a top broker, an underwriting roundtable, or a coverage discussion with an account team all represent meaningful interactions that should be visible in the CRM. This story adds a `Meeting` event type to the CommunicationEvent model. Meetings differ from calls in that they support an `Internal` direction for team discussions about an entity, and a Subject field to capture the meeting agenda or title.

## Acceptance Criteria

**Happy Path:**
- **Given** a distribution user with `communication:create` permission
- **When** they submit a log request with EventType=`Meeting`, Direction, DurationMinutes, Body, PrimaryEntityType, and PrimaryEntityId
- **Then** a CommunicationEvent of type `Meeting` is created, an ActivityTimelineEvent (`CommunicationLogged`) is appended, and the event ID is returned

**Internal Meeting:**
- **Given** the user selects Direction=`Internal`
- **When** the record is created
- **Then** direction is stored as `Internal` and the meeting card shows "Internal" as the direction badge

**Meeting Card Display:**
- **Given** a logged meeting exists
- **When** the Communications feed is viewed
- **Then** the meeting card shows a meeting icon, direction badge, duration, subject (when present), body, and outcome (when present)

**Alternative Flows / Edge Cases:**
- Direction missing → HTTP 400 with `code=direction_required`
- Direction not in `Inbound | Outbound | Internal` → HTTP 400 with `code=invalid_direction`
- DurationMinutes missing → HTTP 400 with `code=duration_required`
- DurationMinutes ≤ 0 or > 1440 (24 hours) → HTTP 400 with `code=duration_invalid`
- Body missing → HTTP 400 with `code=body_required`
- OccurredAt in the future → HTTP 400 with `code=occurred_at_future`
- User lacks permission → HTTP 403
- BrokerUser role → HTTP 403

**Checklist:**
- [ ] EventType = `Meeting` on created record
- [ ] Direction accepts Inbound, Outbound, and Internal
- [ ] DurationMinutes required, 1–1440
- [ ] Subject optional (max 200 chars)
- [ ] Outcome optional (max 1000 chars)
- [ ] ActivityTimelineEvent (`CommunicationLogged`) appended atomically
- [ ] Meeting card renders meeting icon and direction badge in feed
- [ ] All validation errors use RFC 7807 ProblemDetails format

## Data Requirements

**Required Fields:**
- `eventType` (enum): `Meeting`
- `direction` (enum): `Inbound | Outbound | Internal`
- `durationMinutes` (int): Meeting duration in minutes, 1–1440
- `body` (string, max 4000 chars): Meeting notes or summary
- `primaryEntityType` (enum): `Broker | Account | Submission | Policy | Renewal`
- `primaryEntityId` (uuid): ID of the linked entity

**Optional Fields:**
- `subject` (string, max 200 chars): Meeting title or agenda topic
- `outcome` (string, max 1000 chars): Decisions reached or next steps agreed
- `occurredAt` (datetime): When the meeting occurred; defaults to now

**Validation Rules:**
- Direction must be `Inbound`, `Outbound`, or `Internal`
- DurationMinutes must be an integer in the range 1–1440
- Body is required, 1–4000 characters
- Subject, when provided, must be 1–200 characters
- Outcome, when provided, must be 1–1000 characters
- OccurredAt, when provided, must not be in the future

## Role-Based Visibility

**Roles that can create:**
- Distribution User — Full create access
- Underwriter — Full create access
- Distribution Manager — Full create access
- Admin — Full create access

**Roles that cannot create:**
- BrokerUser — HTTP 403

**Data Visibility:**
- InternalOnly: All meeting records are internal in MVP

## Non-Functional Expectations

- Performance: Meeting creation completes in < 500ms
- Security: Entity access check (ABAC) runs before creation
- Reliability: CommunicationEvent and ActivityTimelineEvent created atomically

## Dependencies

**Depends On:**
- F0021-S0001 — Establishes the shared CommunicationEvent model

**Related Stories:**
- F0021-S0001 — Note variant
- F0021-S0002 — Call variant
- F0021-S0004 — Displays meetings in the Communications feed

## Business Rules

1. **Internal Direction Allowed for Meetings Only:** Unlike calls, meetings may be internal (e.g., a strategy discussion about an account with no external participants). `Internal` is a valid Direction value for meetings and is not available for calls.
2. **Duration Cap:** DurationMinutes is capped at 1440 (24 hours) to allow full-day workshop entries. Values outside 1–1440 are rejected.
3. **Subject Strongly Encouraged:** Subject is optional but the UI should visually prompt for it when EventType is Meeting, as untitled meetings are harder to scan in the feed.
4. **Shared Audit Pattern:** ActivityTimelineEvent append is atomic. BrokerDescription is null in MVP.
5. **Entity Access Required:** User must have read access to the linked entity before logging a meeting against it.

## Out of Scope

- Calendar integration or meeting invite creation (deferred to F0030)
- Structured participant list (free-text body covers this for MVP)
- Video conference link capture
- Recurring meeting tracking

## UI/UX Notes

- Screens involved: Log Communication modal → "Meeting" tab, Communications feed card
- Key interactions: User selects "Meeting" tab, picks direction (Inbound/Outbound/Internal toggle), enters duration, optionally fills subject, fills body and outcome, optionally adjusts OccurredAt
- Feed card: meeting icon, direction badge pill, "{N} min" duration, subject in semi-bold (when present), body text, outcome in muted style
- Subject field is more prominent on the Meeting tab than on the Note tab

## Questions & Assumptions

**Open Questions:**
- [ ] Should we capture participant names as a structured list? Deferred — free-text body covers this for MVP.

**Assumptions (to be validated):**
- Internal direction is valid for meetings but not for calls
- Duration cap of 24 hours (1440 min) is sufficient for any realistic meeting scenario
- Subject is optional but the UI will visually encourage it for meeting entries

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Edge cases handled (Internal direction, duration limits, future OccurredAt, BrokerUser 403)
- [ ] Permissions enforced
- [ ] Audit/timeline logged atomically
- [ ] Tests pass
- [ ] Story filename matches `Story ID` prefix (`F0021-S0003-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
