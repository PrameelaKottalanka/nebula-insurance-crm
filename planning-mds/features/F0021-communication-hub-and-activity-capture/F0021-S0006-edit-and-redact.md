---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0006: Edit and Redact a Communication Event

**Story ID:** F0021-S0006
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Edit and redact a communication event
**Priority:** High
**Phase:** CRM Release MVP

## User Story

**As a** distribution user
**I want** to correct a mistake in my own note or call log within 24 hours of creating it
**And as an** admin
**I want** to redact the body of any communication event at any time
**So that** records stay accurate and sensitive content can be removed while the audit trail is preserved

## Context & Background

Communication events are append-only by design — they are not simple records users can freely edit or delete. However, two controlled modification paths are needed. First, authors should be able to fix typos or factual errors in their own records within a short window (24 hours), after which the record is considered settled. Second, admins must be able to redact the body of any event (e.g., if a note accidentally contains PII, privileged information, or a compliance-sensitive statement) without deleting the event entirely. Both operations are logged as ActivityTimelineEvents so the audit trail is never broken.

## Acceptance Criteria

**Edit — Happy Path:**
- **Given** a distribution user who authored a communication event less than 24 hours ago
- **When** they submit an edit request with an updated body (and optionally subject, outcome, or occurredAt)
- **Then** the event is updated, LastEditedAt and LastEditedByUserId are set, an ActivityTimelineEvent (`CommunicationEdited`) is appended to the linked entity, and the card displays "Edited {timestamp}" beneath the body

**Edit — Window Expired:**
- **Given** a communication event was created more than 24 hours ago
- **When** any non-Admin user attempts to edit it
- **Then** HTTP 403 is returned with `code=edit_window_expired`

**Edit — Not Author:**
- **Given** a user who is not the event author and not an Admin
- **When** they attempt to edit the event
- **Then** HTTP 403 is returned with `code=not_author`

**Admin Edit — No Window:**
- **Given** an Admin user
- **When** they submit an edit on any event regardless of age
- **Then** the edit is accepted; LastEditedAt, LastEditedByUserId updated; ActivityTimelineEvent appended

**Redact — Happy Path:**
- **Given** an Admin user
- **When** they submit a redact request with a RedactionReason
- **Then** IsRedacted is set to true, Body is cleared (set to null/empty), RedactedAt, RedactedByUserId, and RedactionReason are recorded, and an ActivityTimelineEvent (`CommunicationRedacted`) is appended

**Redact — Card Display:**
- **Given** an event has been redacted
- **When** the Communications feed is viewed
- **Then** the body area shows `[Redacted by {AdminDisplayName} on {RedactedAt date}]`; Outcome is hidden; the event card is visually muted

**Alternative Flows / Edge Cases:**
- Edit on a redacted event → HTTP 409 with `code=event_redacted` (redacted events cannot be edited)
- Redact on an already-redacted event → HTTP 409 with `code=already_redacted`
- RedactionReason missing on redact request → HTTP 400 with `code=reason_required`
- Non-Admin attempts to redact → HTTP 403 with `code=insufficient_role`
- Edit changes no fields → HTTP 400 with `code=no_changes`

**Checklist:**
- [ ] Edit button visible on card only to the event author and only within 24h
- [ ] Edit accepted from Admin at any time regardless of age
- [ ] LastEditedAt and LastEditedByUserId set on successful edit
- [ ] "Edited {timestamp}" shown beneath body on edited cards
- [ ] ActivityTimelineEvent (`CommunicationEdited`) appended atomically on edit
- [ ] Redact accepted from Admin only
- [ ] Body cleared on redact; RedactedAt, RedactedByUserId, RedactionReason recorded
- [ ] ActivityTimelineEvent (`CommunicationRedacted`) appended atomically on redact
- [ ] Redacted card renders body placeholder and Outcome hidden
- [ ] Edit on redacted event → 409; Redact on already-redacted → 409
- [ ] All error responses use RFC 7807 ProblemDetails format

## Data Requirements

**Edit Request (PUT /api/communications/{id}):**
- `body` (string, optional, max 4000 chars): Updated body
- `subject` (string, optional, max 200 chars): Updated subject
- `outcome` (string, optional, max 1000 chars): Updated outcome
- `occurredAt` (datetime, optional): Corrected timestamp; must not be in the future

**Redact Request (POST /api/communications/{id}/redact):**
- `reason` (string, required, max 500 chars): Why the body is being redacted

**Fields updated on edit:**
- `Body`, `Subject`, `Outcome`, `OccurredAt` (whichever are provided)
- `LastEditedAt` = now, `LastEditedByUserId` = current user

**Fields updated on redact:**
- `IsRedacted` = true, `Body` = null, `RedactedAt` = now, `RedactedByUserId` = current user, `RedactionReason` = reason

## Role-Based Visibility

**Edit:**
- Author (within 24h) — Can edit own event body, subject, outcome, occurredAt
- Admin — Can edit any event at any time

**Redact:**
- Admin only — Redact any event at any time; no time restriction

**Non-Admin, non-author:** HTTP 403 on both edit and redact

## Non-Functional Expectations

- Performance: Edit and redact complete in < 500ms
- Security: Author check and 24h window enforced server-side; Admin role required for redact; redaction reason is mandatory and stored for compliance
- Reliability: Event update and ActivityTimelineEvent appended atomically

## Dependencies

**Depends On:**
- F0021-S0001 — Communication events must exist to be edited or redacted
- F0021-S0004 — Edited and redacted states are rendered in the feed

## Business Rules

1. **24-Hour Edit Window for Non-Admin Authors:** Authors may edit their own events within 24 hours of creation. After that, only Admins may edit. The window is calculated server-side using `CreatedAt`.
2. **Redacted Events Cannot Be Edited:** If IsRedacted=true, the body is already cleared. Editing a redacted body is meaningless and is rejected with HTTP 409.
3. **Redaction Is Irreversible in MVP:** Once IsRedacted=true, there is no un-redact operation. The body is gone. This is intentional for compliance purposes.
4. **Redaction Reason Required:** A reason must be provided on every redact request. It is stored on the record and visible to Admins in the card metadata.
5. **Audit Trail Preserved on Both Operations:** Both edit and redact append an ActivityTimelineEvent. The timeline always reflects that a modification occurred, even when the content is gone.
6. **OccurredAt Can Be Corrected on Edit:** If a user logged a call with the wrong time, they may correct OccurredAt within the edit window. The corrected timestamp must still not be in the future.

## Out of Scope

- Un-redacting a redacted event (irreversible in MVP)
- Edit history / version diff view (body before/after)
- Author editing beyond 24h without Admin override
- Bulk redaction
- Redaction notification to the event author

## UI/UX Notes

- Screens involved: Communications feed card (Edit button, overflow menu with Redact)
- Edit interaction: "Edit" button visible below body on own cards within 24h → opens an inline edit form or modal pre-filled with existing values
- Redact interaction: Admin sees "···" overflow menu on any card → "Redact..." opens a confirmation modal with a required Reason field
- After edit: card body updates in place; "Edited {timestamp}" label appears in muted text beneath body
- After redact: card body replaced with placeholder text; card visually muted

## Questions & Assumptions

**Open Questions:**
- [ ] Should non-admin authors be able to delete (not just redact) their own notes within the 24h window? Assumption: no — deletion is not supported in MVP; edit-within-window is the correction mechanism.

**Assumptions (to be validated):**
- 24-hour edit window is the right balance between correction flexibility and audit integrity
- Redaction is admin-only; authors cannot self-redact (they can edit within 24h instead)
- Redaction is irreversible in MVP

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Edit window enforced server-side (24h for non-admin authors; unlimited for Admins)
- [ ] Redact enforced to Admin role only
- [ ] Both operations emit ActivityTimelineEvent atomically
- [ ] Edited card shows "Edited {timestamp}" label
- [ ] Redacted card shows body placeholder and Outcome hidden
- [ ] All edge cases handled (redacted→edit 409, already-redacted 409, non-author 403, window-expired 403)
- [ ] Tests pass
- [ ] Story filename matches `Story ID` prefix (`F0021-S0006-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
