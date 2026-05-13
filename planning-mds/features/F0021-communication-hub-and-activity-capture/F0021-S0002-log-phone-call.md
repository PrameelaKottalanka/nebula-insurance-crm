---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0002: Log a Phone Call

**Story ID:** F0021-S0002
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Log a phone call
**Priority:** Critical
**Phase:** CRM Release MVP

## User Story

**As a** distribution user or underwriter
**I want** to log a phone call with direction, duration, and outcome against a CRM entity
**So that** call history is part of the broker or account record and does not live only in personal notes or Outlook

## Context & Background

Phone calls are the primary communication channel in commercial P&C insurance. Distribution users call brokers to discuss submissions, renewals, and account issues daily. Without a structured call log, this history is invisible to colleagues and managers. This story adds a `Call` event type to the CommunicationEvent model established in S0001, requiring direction and duration in addition to the shared note fields.

## Acceptance Criteria

**Happy Path:**
- **Given** a distribution user with `communication:create` permission
- **When** they submit a log request with EventType=`Call`, Direction, DurationMinutes, Body, PrimaryEntityType, and PrimaryEntityId
- **Then** a CommunicationEvent of type `Call` is created with all fields persisted, an ActivityTimelineEvent (`CommunicationLogged`) is appended to the linked entity, and the event ID is returned

**Call Card Display:**
- **Given** a logged call exists on an entity
- **When** the Communications feed is viewed
- **Then** the call card shows a phone icon, direction badge (Inbound / Outbound), duration in minutes, body text, and outcome (when present)

**Alternative Flows / Edge Cases:**
- Direction missing → HTTP 400 with `code=direction_required`
- Direction value not in `Inbound | Outbound` → HTTP 400 with `code=invalid_direction`
- DurationMinutes missing → HTTP 400 with `code=duration_required`
- DurationMinutes ≤ 0 → HTTP 400 with `code=duration_invalid`
- DurationMinutes > 600 (10 hours) → HTTP 400 with `code=duration_too_long`
- Body missing → HTTP 400 with `code=body_required`
- OccurredAt in the future → HTTP 400 with `code=occurred_at_future`
- User lacks `communication:create` permission → HTTP 403
- BrokerUser role → HTTP 403

**Checklist:**
- [ ] EventType = `Call` on created record
- [ ] Direction (Inbound/Outbound) required and validated
- [ ] DurationMinutes required, must be 1–600
- [ ] Outcome persisted when provided (optional)
- [ ] ActivityTimelineEvent (`CommunicationLogged`) appended atomically
- [ ] Call card renders phone icon and direction badge in the Communications feed
- [ ] All validation errors use RFC 7807 ProblemDetails format

## Data Requirements

**Required Fields:**
- `eventType` (enum): `Call`
- `direction` (enum): `Inbound | Outbound`
- `durationMinutes` (int): Call duration in minutes, 1–600
- `body` (string, max 4000 chars): Call summary / notes
- `primaryEntityType` (enum): `Broker | Account | Submission | Policy | Renewal`
- `primaryEntityId` (uuid): ID of the linked entity

**Optional Fields:**
- `subject` (string, max 200 chars): Short call title
- `outcome` (string, max 1000 chars): Result or next step agreed on the call
- `occurredAt` (datetime): When the call happened; defaults to now

**Validation Rules:**
- Direction must be `Inbound` or `Outbound` (Internal is not valid for calls)
- DurationMinutes must be an integer in the range 1–600
- Body is required, 1–4000 characters
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
- InternalOnly: All call records are internal in MVP

## Non-Functional Expectations

- Performance: Call creation completes in < 500ms
- Security: Entity access check (ABAC) runs before creation; user must have read access to the linked entity
- Reliability: CommunicationEvent and ActivityTimelineEvent created atomically

## Dependencies

**Depends On:**
- F0021-S0001 — Establishes the CommunicationEvent model; this story adds the Call variant

**Related Stories:**
- F0021-S0001 — Note variant (shared model)
- F0021-S0003 — Meeting variant
- F0021-S0004 — Displays calls in the Communications feed

## Business Rules

1. **Direction Required for Calls:** Calls must be tagged as Inbound or Outbound. `Internal` is not valid for calls (reserved for meetings). This distinction matters for broker relationship analytics.
2. **Duration Cap:** DurationMinutes is capped at 600 (10 hours) to catch data entry errors. Values outside 1–600 are rejected.
3. **Outcome Is Optional:** Many quick calls have no formal outcome to record. The field is available but not required.
4. **Shared Audit Pattern:** ActivityTimelineEvent append follows the same atomic pattern as S0001. BrokerDescription is null in MVP.
5. **Entity Access Required:** User must have read access to the linked entity before creating a call log entry.

## Out of Scope

- Call recording or audio attachment
- Telephony system integration (deferred to F0030)
- Automatic call detection or CTI pop-up
- Internal direction for calls (Internal is for meetings only)

## UI/UX Notes

- Screens involved: Log Communication modal → "Call" tab, Communications feed card
- Key interactions: User selects "Call" tab, picks direction (Inbound/Outbound toggle), enters duration, fills body, optionally adds outcome and OccurredAt
- Feed card: phone icon, direction badge pill, "{N} min" duration, body text, outcome in muted style below body

## Questions & Assumptions

**Open Questions:**
- [ ] Should calls support a structured participant list? Deferred — free-text notes in body cover this for MVP.

**Assumptions (to be validated):**
- Direction is always Inbound or Outbound; no Internal calls in MVP
- Duration is a manually entered integer in minutes; no auto-detection
- Outcome is free text; no structured outcome codes in MVP

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Edge cases handled (missing direction, invalid duration, future OccurredAt, BrokerUser 403)
- [ ] Permissions enforced
- [ ] Audit/timeline logged atomically
- [ ] Tests pass
- [ ] Story filename matches `Story ID` prefix (`F0021-S0002-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
