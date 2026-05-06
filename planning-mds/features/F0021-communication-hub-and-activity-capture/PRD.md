---
template: feature
version: 1.1
applies_to: product-manager
---

# F0021: Communication Hub & Activity Capture

**Feature ID:** F0021
**Feature Name:** Communication Hub & Activity Capture
**Priority:** High
**Phase:** CRM Release MVP

## Feature Statement

**As a** distribution user, coordinator, or underwriter
**I want** communication history captured in Nebula
**So that** broker and customer interactions are visible, auditable, and actionable without relying on memory or email search

## Business Objective

- **Goal:** Move communication history into the CRM operating record.
- **Metric:** Captured activity volume, follow-up completion, and reduced time spent reconstructing conversations.
- **Baseline:** Important communication lives in Outlook threads and private notes.
- **Target:** Users can review meaningful communication history directly from Nebula records.

## Problem Statement

- **Current State:** Communication trails are fragmented and difficult to audit.
- **Desired State:** Calls, meetings, notes, and email-linked activity are visible in context.
- **Impact:** Faster follow-up, better broker service, and stronger institutional memory.

## Scope & Boundaries

**In Scope:**
- Notes, calls, meetings, and communication events
- Related activity capture on broker, account, submission, and policy records
- Follow-up creation and activity linkage
- Communication timeline visibility

**Out of Scope:**
- Full email-sending client
- Marketing automation
- External messaging integrations beyond the agreed MVP scope

## Success Criteria

- Users can see relevant communication history in context.
- Communication capture supports task follow-up and relationship continuity.
- Activity history reduces dependence on inbox archaeology.

## Risks & Assumptions

- **Risk:** Communication scope expands into a full messaging platform too early.
- **Assumption:** Structured activity capture is more important than full outbound email functionality in the first release.
- **Mitigation:** Focus on capture, visibility, and linkage before deeper integrations.

## Dependencies

- F0016 Account 360 & Insured Management
- F0004 Task Center UI + Manager Assignment

## Architecture & Solution Design

### Solution Components

- Introduce a communication or activity-capture service that owns notes, calls, meetings, and communication events as append-only business records.
- Add follow-up linkage between communication records and task or reminder capabilities instead of forcing communication state into the task schema itself.
- Provide timeline composition services that can render communication history consistently on broker, account, submission, and policy views.
- Keep full email-client behavior and broad messaging integrations out of the initial component set.

### Data & Workflow Design

- Model communication events with type, subject or summary, participants, occurred-at timestamp, linked entity references, and follow-up requirements.
- Preserve immutable timeline history for captured activity while allowing controlled correction or redaction workflows where necessary.
- Link communication records to multiple business objects where appropriate, but define a clear primary entity for ownership and authorization evaluation.
- Reuse task identifiers for follow-up tracking rather than duplicating a second workflow engine for basic reminders.

### API & Integration Design

- Expose endpoints for creating and retrieving communication events, linking them to entities, and creating associated follow-up tasks.
- Design the contract so later integrations with email, calendar, or telephony systems can map into the same communication event model.
- Reuse existing activity timeline patterns and descriptions so downstream reporting can treat communication as another auditable event source.
- Keep the API focused on structured capture and retrieval rather than trying to become a general-purpose messaging platform.

### Security & Operational Considerations

- Apply authorization from the linked business records and respect visibility constraints for sensitive communication notes.
- Support audit logging for creation, edit, redaction, and follow-up completion actions because communication history often becomes evidence in account handling.
- Guard against duplicate ingestion when later connectors sync the same meeting or email more than once.
- Index activity queries by primary entity, participant, event type, and occurred-at date because communication timelines can grow rapidly.

## Architecture Traceability

**Taxonomy Reference:** [Feature Architecture Traceability Taxonomy](../../architecture/feature-architecture-traceability-taxonomy.md)

| Classification | Artifact / Decision | ADR |
|----------------|---------------------|-----|
| Introduces: Feature-Local Component | Communication capture service, follow-up linkage, and communication timeline composition | PRD only |
| Extends: Cross-Cutting Component | Communication events become integration-friendly records for external exchange and replay | [ADR-015](../../architecture/decisions/ADR-015-integration-hub-canonical-contracts-and-outbox.md) (Proposed) |
| Reuses: Established Component/Pattern | Append-only activity timeline and follow-up linkage patterns | PRD only |

## UX / Screens

| Screen | Purpose | Key Actions |
|--------|---------|-------------|
| Communications tab (on entity detail) | View chronological communication feed | Browse, paginate, open log modal |
| Log Communication modal | Capture note, call, or meeting | Select type, fill fields, optionally create follow-up |
| Create Follow-Up Task modal | Create a task linked to a communication | Fill title, due date, assignee |
| Edit Communication (inline/modal) | Correct an existing event within 24h | Update body, subject, outcome, occurredAt |
| Redact Communication modal | Remove sensitive body content (Admin only) | Enter reason, confirm |

**Key Workflows:**
1. Log Note — User opens modal, selects Note, enters body, clicks "Log Communication" → card appears in feed
2. Log Call with Follow-Up — User selects Call, fills direction + duration + body, checks follow-up, saves → follow-up modal → task created → card shows task link
3. Edit Within Window — Author clicks Edit on own card within 24h → updates body → "Edited" label appears
4. Admin Redact — Admin opens ··· menu, clicks Redact, enters reason → body replaced with placeholder

## Screen Layouts (ASCII)

### Communications Tab — Desktop

```
┌─────────────────────────────────────────────────────────────────────┐
│  ← Acme Corp   [Broker: Summit Insurance]           [+ Log Comms ▾] │
├─────────────────────────────────────────────────────────────────────┤
│  Overview  Contacts  Submissions  Timeline  [Communications]         │
├─────────────────────────────────────────────────────────────────────┤
│  Type: [All ▾]   Author: [All ▾]   Date: [Last 90 days ▾]  [Clear] │
├─────────────────────────────────────────────────────────────────────┤
│  ── MAY 2026 ─────────────────────────────────────────────────────  │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ 📞 Phone Call · May 5, 2026 · 2:30 PM · Jane Smith          │   │
│  │ Outbound · 15 min                                            │   │
│  │ Discussed Q2 renewal terms. Broker will send exposure data.  │   │
│  │ Outcome: Confirmed receipt by Friday.                        │   │
│  │ ↳ Follow-up: [Send renewal proposal →]                       │   │
│  │                                       [Edit]  [···]          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ 📝 Note · May 4, 2026 · 11:00 AM · Jane Smith               │   │
│  │ Broker confirmed no prior losses for this policy period.     │   │
│  │ Edited 11:42 AM                       [Edit]  [···]          │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ── APRIL 2026 ────────────────────────────────────────────────     │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ 🤝 Meeting · Apr 28, 2026 · 10:00 AM · Tom Lee              │   │
│  │ Internal · 60 min · Quarterly broker strategy review         │   │
│  │ Outcome: Agreed to expand into GL line at next renewal.      │   │
│  │                                                  [···]        │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ 📝 Note · Apr 15, 2026 · 3:00 PM · [Redacted by Admin]      │   │
│  │ [Redacted by Admin User on Apr 16, 2026]                     │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│                        [Load earlier communications]                 │
└─────────────────────────────────────────────────────────────────────┘
```

### Log Communication Modal

```
┌────────────────────────────────────────────────┐
│  Log Communication                        [✕]   │
├────────────────────────────────────────────────┤
│  TYPE                                           │
│  [📝 Note]  [📞 Call]  [🤝 Meeting]            │
│                                                 │
│  ── (Call or Meeting only) ──────────────────   │
│  Direction              Duration                │
│  ○ Outbound  ○ Inbound  [____] minutes          │
│  (Meeting also: ○ Internal)                     │
│  ──────────────────────────────────────────     │
│  When did this occur?                           │
│  Date [May 5, 2026 ▾]   Time [2:30 PM ▾]       │
│                                                 │
│  Subject (optional)                             │
│  [____________________________________________] │
│                                                 │
│  Notes *                                        │
│  ┌──────────────────────────────────────────┐   │
│  │                                          │   │
│  │                                          │   │
│  └──────────────────────────────────────────┘   │
│                                                 │
│  Outcome (optional — calls & meetings)          │
│  [____________________________________________] │
│                                                 │
│  ☐ Create a follow-up task after saving         │
│                                                 │
│                 [Cancel]  [Log Communication]   │
└────────────────────────────────────────────────┘
```

### Create Follow-Up Task Modal (appears after saving)

```
┌────────────────────────────────────────────────┐
│  Create Follow-Up Task                    [✕]   │
├────────────────────────────────────────────────┤
│  Linked to: Acme Corp · Phone Call (May 5)      │
│                                                 │
│  Task Title *                                   │
│  [____________________________________________] │
│                                                 │
│  Due Date                  Priority             │
│  [May 12, 2026 ▾]          [Normal ▾]           │
│                                                 │
│  Assign To                                      │
│  [Jane Smith (me) ▾]                            │
│                                                 │
│                    [Skip]  [Create Task]        │
└────────────────────────────────────────────────┘
```

### Communications Tab — Mobile

```
┌──────────────────────────┐
│  ← Acme Corp   [+ Log ▾] │
├──────────────────────────┤
│  [Communications]        │
│  [Filter ▾]              │
├──────────────────────────┤
│  ── MAY 2026 ──────────  │
│                          │
│  ┌──────────────────┐    │
│  │ 📞 Call · May 5  │    │
│  │ Outbound · 15min │    │
│  │ Discussed Q2...  │    │
│  │ [Edit]  [···]    │    │
│  └──────────────────┘    │
│                          │
│  ┌──────────────────┐    │
│  │ 📝 Note · May 4  │    │
│  │ Broker confirmed │    │
│  │ no prior losses  │    │
│  │ [Edit]  [···]    │    │
│  └──────────────────┘    │
│                          │
│  [Load earlier]          │
└──────────────────────────┘
```

## Related User Stories

- [F0021-S0001](./F0021-S0001-log-communication-note.md) — Log a communication note
- [F0021-S0002](./F0021-S0002-log-phone-call.md) — Log a phone call
- [F0021-S0003](./F0021-S0003-log-meeting.md) — Log a meeting
- [F0021-S0004](./F0021-S0004-view-communication-feed.md) — View communication feed on entity
- [F0021-S0005](./F0021-S0005-create-follow-up-task.md) — Create follow-up task from communication
- [F0021-S0006](./F0021-S0006-edit-and-redact.md) — Edit and redact a communication event
- [F0021-S0007](./F0021-S0007-filter-communication-feed.md) — Filter communication feed by type and date (Phase 1)
