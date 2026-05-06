---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0004: View Communication Feed on Entity

**Story ID:** F0021-S0004
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** View communication feed on entity
**Priority:** Critical
**Phase:** CRM Release MVP

## User Story

**As an** internal user working on a broker, account, submission, policy, or renewal
**I want** to see a chronological feed of all communication events on that record
**So that** I can understand the interaction history without searching inboxes or asking colleagues

## Context & Background

The Communications feed is the read-side of the feature. It surfaces all notes, calls, and meetings logged against an entity on a dedicated tab of the entity detail page. Events are sorted newest first and grouped by calendar month to aid scanning. The feed handles redacted events gracefully and shows an informative empty state when no communications exist. Pagination prevents long-tenured records from loading slowly.

## Acceptance Criteria

**Happy Path:**
- **Given** an internal user viewing an entity detail page with at least one communication event
- **When** they click the "Communications" tab
- **Then** events are displayed sorted by OccurredAt descending, grouped by calendar month, showing 25 per page

**Empty State:**
- **Given** no communication events exist for the entity
- **When** the Communications tab is viewed
- **Then** the message "No communications logged yet. Log the first one." is shown with a prompt to open the log modal

**Redacted Event:**
- **Given** a communication event has IsRedacted=true
- **When** the tab is viewed
- **Then** the card shows event type, timestamp, and author, but body is replaced with `[Redacted by {AdminDisplayName} on {RedactedAt date}]` and Outcome is hidden

**Pagination:**
- **Given** more than 25 events exist
- **When** the first page loads
- **Then** the 25 most recent are shown and a "Load earlier communications" button appears at the bottom; clicking it appends the next 25 without replacing already-loaded cards

**Alternative Flows / Edge Cases:**
- Entity not found → HTTP 404
- User lacks read access to the entity → HTTP 403; tab not shown in navigation
- All events are redacted → feed shows only placeholder cards, no empty state
- BrokerUser role → Communications tab not shown in MVP (all events are internal-only)

**Checklist:**
- [ ] Communications tab added to: Account detail, Broker detail, Submission detail, Policy detail, Renewal detail pages
- [ ] Events sorted OccurredAt descending
- [ ] Events grouped by calendar month with a visible month/year heading
- [ ] Note cards: body text displayed
- [ ] Call cards: direction badge, "{N} min", body, outcome (when present)
- [ ] Meeting cards: direction badge, "{N} min", subject (when present, semi-bold), body, outcome (when present)
- [ ] Redacted cards: placeholder text instead of body; Outcome field hidden
- [ ] Empty state displayed when no events exist
- [ ] Pagination: 25 per page, "Load earlier" button appends next page
- [ ] BrokerUser: Communications tab not rendered in navigation (all events are internal-only in MVP)

## Data Requirements

**API:** `GET /api/{entityType}/{entityId}/communications?page=1&pageSize=25`

**Response shape:**
```
{
  data: CommunicationEventSummary[],
  page: int,
  pageSize: int,
  totalCount: int,
  totalPages: int
}
```

**CommunicationEventSummary includes:**
- `id`, `eventType`, `direction`, `durationMinutes`, `subject`, `body` (null when redacted), `outcome`
- `occurredAt`, `authoredByUserId`, `authoredByDisplayName`
- `isRedacted`, `redactedByDisplayName`, `redactedAt`
- `lastEditedAt`
- `followUpTaskId`, `followUpTaskTitle`, `followUpTaskStatus`

## Role-Based Visibility

**Roles that can view:**
- Distribution User, Underwriter, Distribution Manager, Admin — Full read access to all non-redacted events on accessible entities
- Admin — Can also see redacted event metadata (type, date, author, redactor)

**BrokerUser:**
- Tab hidden in MVP; no communication events have a BrokerDescription in MVP

## Non-Functional Expectations

- Performance: First page loads in < 1s; indexed by PrimaryEntityType + PrimaryEntityId + OccurredAt DESC
- Security: Entity-level ABAC access check runs before returning events
- Accessibility: Month group headings use semantic heading elements; event cards are keyboard-navigable

## Dependencies

**Depends On:**
- F0021-S0001 — Notes to display
- F0021-S0002 — Call-specific fields (direction, duration)
- F0021-S0003 — Meeting-specific fields (direction, duration, subject)

**Related Stories:**
- F0021-S0005 — Follow-up task link shown on event cards
- F0021-S0006 — Redacted and edited event states rendered here
- F0021-S0007 — Filter bar added to this feed

## Business Rules

1. **Newest First:** Events always sorted by OccurredAt descending within each page. Most recent interaction appears at the top.
2. **Month Grouping:** Events grouped by calendar month (e.g., "May 2026", "April 2026") with a visible heading. Grouping is client-side based on OccurredAt.
3. **Redacted Body Replacement:** When IsRedacted=true, body is null in the API response. Frontend renders `[Redacted by {redactedByDisplayName} on {redactedAt}]`. Outcome is also hidden for redacted events.
4. **BrokerUser Exclusion:** In MVP, all communication events have null BrokerDescription. The Communications tab is not shown in BrokerUser navigation, consistent with broker-scope isolation from F0009.
5. **Soft-Deleted Entities:** Communication history for soft-deleted entities (e.g., a withdrawn submission) remains accessible for audit purposes. The feed does not hide events based on entity deletion status.

## Out of Scope

- Full-text search within the feed (deferred to F0023 Global Search)
- Filtering by type or author (S0007)
- Cross-entity feed (e.g., all communications for an account including its linked submissions)
- Export of communication history to CSV or PDF

## UI/UX Notes

- Screens involved: Account detail, Broker detail, Submission detail, Policy detail, Renewal detail — each adds a "Communications" tab
- Tab label: "Communications" (count badge added in S0007)
- Empty state: centered message with a CTA button to open the log modal
- Redacted card: muted/greyed visual style to distinguish from normal events

## Questions & Assumptions

**Open Questions:**
- [ ] Should the tab appear on all five entity types from day one, or phase Submission/Policy/Renewal in later? Assumption: all five entity types from the start.

**Assumptions (to be validated):**
- 25 events per page; "Load earlier" append pattern preferred over traditional pagination controls for a feed
- Month grouping is client-side based on OccurredAt returned by the API
- BrokerUser tab exclusion is the correct MVP decision; broker-visible communications deferred to a later feature

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Communications tab functional on all five entity detail pages
- [ ] Empty state, redacted card, and paginated load-more all working
- [ ] BrokerUser: tab not rendered in navigation
- [ ] Permissions enforced (entity ABAC access check)
- [ ] Tests pass (component tests + integration test for pagination and redacted display)
- [ ] Story filename matches `Story ID` prefix (`F0021-S0004-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
