---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0007: Filter Communication Feed by Type and Date

**Story ID:** F0021-S0007
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Filter communication feed by type and date
**Priority:** Medium
**Phase:** Phase 1

## User Story

**As an** internal user reviewing a broker or account with a long communication history
**I want** to filter the feed by event type and date range
**So that** I can find specific calls, meetings, or notes without scrolling through the entire history

## Context & Background

As communication history accumulates on high-volume entities (busy broker accounts, large renewals), the unfiltered feed becomes hard to navigate. This story adds a filter bar above the feed that lets users narrow by type (Note, Call, Meeting), author, and date range. Filter state is reflected in URL query parameters so filtered views can be bookmarked or shared. A count badge on the tab label signals when filters are active.

## Acceptance Criteria

**Filter by Type:**
- **Given** a user selects "Call" from the Type filter
- **When** the feed refreshes
- **Then** only CommunicationEvents with EventType=`Call` are shown

**Filter by Date Range:**
- **Given** a user selects "Last 30 days" from the Date filter
- **When** the feed refreshes
- **Then** only events with OccurredAt within the last 30 days are shown

**Combined Filters:**
- **Given** a user selects Type=`Meeting` and Date=`Last 90 days`
- **When** the feed refreshes
- **Then** only meeting events within the last 90 days are shown

**URL Persistence:**
- **Given** a user applies filters
- **When** the URL is copied and opened in a new tab
- **Then** the same filter state is restored and the feed shows the same results

**Clear Filters:**
- **Given** one or more filters are active
- **When** the user clicks "Clear"
- **Then** all filters reset to their defaults and the full feed reloads

**Tab Badge:**
- **Given** one or more filters are active
- **When** the user views the tab label
- **Then** the label shows "Communications ({count})" where count reflects the filtered result count

**Alternative Flows / Edge Cases:**
- No events match the applied filters → empty state message "No communications match your filters." with a "Clear filters" link
- Invalid date range in URL params → filters ignored, full feed loaded
- All types deselected → treated as "All" (no type filter applied)

**Checklist:**
- [ ] Filter bar visible above the feed on all five entity detail pages
- [ ] Type filter: multi-select options (All, Note, Call, Meeting)
- [ ] Author filter: dropdown of users who have logged events on this entity
- [ ] Date range presets: Last 30 days, Last 90 days, Last 180 days, All time (default)
- [ ] Filters applied via updated API query params (not client-side filtering)
- [ ] Filter state persisted to URL query params
- [ ] "Clear" button resets all filters
- [ ] Tab label badge shows filtered count when any filter is active
- [ ] Empty state shows "No communications match your filters." with "Clear filters" link
- [ ] Filter bar is hidden or collapsed on mobile (accessible via an expandable panel)

## Data Requirements

**API query parameters added to `GET /api/{entityType}/{entityId}/communications`:**
- `eventType` (string, optional): Comma-separated values — `Note`, `Call`, `Meeting`
- `authorId` (uuid, optional): Filter by authored user
- `dateFrom` (date, optional): OccurredAt >= dateFrom
- `dateTo` (date, optional): OccurredAt <= dateTo

**Author dropdown population:**
- Derived from distinct `authoredByUserId` + `authoredByDisplayName` values on existing events for the entity (no separate API call needed)

## Role-Based Visibility

- All internal roles that can view the feed (Distribution User, Underwriter, Distribution Manager, Admin) can also use the filter bar
- BrokerUser: tab still hidden in MVP; no filter bar shown

## Non-Functional Expectations

- Performance: Filtered queries must use the same index as the base feed (PrimaryEntityType + PrimaryEntityId + OccurredAt DESC); type and date filters should not require a table scan
- Accessibility: Filter controls are keyboard-navigable and labelled with ARIA attributes

## Dependencies

**Depends On:**
- F0021-S0004 — Base feed must be working before filters are added

**Related Stories:**
- F0021-S0004 — Feed that this story adds filters to

## Business Rules

1. **Server-Side Filtering:** Filters are applied server-side via query parameters, not client-side on already-loaded data. This ensures pagination and counts remain accurate.
2. **All Types = No Filter:** If the user selects all three types (or deselects all), the filter is treated as "no type restriction" and the full feed loads. This prevents an empty feed from confusing users who accidentally deselect all options.
3. **Date Presets Map to Server-Side Ranges:** "Last 30 days" maps to `dateFrom = today - 30 days`, `dateTo = today`. The mapping is done client-side before the API call.
4. **URL Reflects Filter State:** Query params `?eventType=Call&dateFrom=2026-04-01&dateTo=2026-05-06` allow filter state to be shared via URL. On load, query params are parsed and applied to the filter bar before the first API call.
5. **Tab Badge Count:** The badge count reflects the totalCount from the filtered API response, not just the count of loaded cards. It resets to no badge when filters are cleared.

## Out of Scope

- Full-text search within the feed (deferred to F0023 Global Search)
- Saving custom filter views (deferred to F0023 Saved Views)
- Filtering by outcome or subject content
- Custom date range picker (presets only in MVP)

## UI/UX Notes

- Screens involved: Communications tab on all five entity detail pages (filter bar added above the feed)
- Filter bar layout (desktop):
  ```
  Type: [All ▾]   Author: [All ▾]   Date: [Last 90 days ▾]   [Clear]
  ```
- Mobile: filter bar collapsed behind a "Filter" button that expands a panel
- Active filter indicator: each active filter control shows a subtle highlight; tab label shows badge count
- "Clear filters" link appears in the empty-state message when no results match

## Questions & Assumptions

**Open Questions:**
- [ ] Should "Author" filter be populated from existing events or from the full user directory? Assumption: from existing events on this entity (simpler, and ensures only relevant authors appear).

**Assumptions (to be validated):**
- Date presets (30 / 90 / 180 days / All time) cover the common use cases; a custom date range picker is deferred
- Author filter population from existing events is sufficient for MVP; full user directory search deferred

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Filter bar functional on all five entity detail pages
- [ ] Type, author, and date range filters work independently and in combination
- [ ] Filter state persisted to URL query params and restored on load
- [ ] Tab badge shows filtered count when filters are active
- [ ] Empty-state "No communications match your filters." shown when applicable
- [ ] Server-side filtering via API query params (not client-side)
- [ ] Tests pass (filter combinations, URL persistence, empty state)
- [ ] Story filename matches `Story ID` prefix (`F0021-S0007-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
