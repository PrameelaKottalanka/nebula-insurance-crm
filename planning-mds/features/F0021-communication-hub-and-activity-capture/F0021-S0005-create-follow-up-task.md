---
template: user-story
version: 1.1
applies_to: product-manager
---

# F0021-S0005: Create Follow-Up Task from Communication

**Story ID:** F0021-S0005
**Feature:** F0021 — Communication Hub & Activity Capture
**Title:** Create follow-up task from communication
**Priority:** High
**Phase:** CRM Release MVP

## User Story

**As a** distribution user
**I want** to create a follow-up task directly from a communication event in one step
**So that** action items arising from calls and meetings are tracked in the Task Center without switching context

## Context & Background

Many calls and meetings produce action items — send a proposal, chase a loss run, follow up on a decision. Without a link from the communication to a task, these items get lost. This story adds a follow-up task flow to the communication log: a checkbox on the Log Communication modal prompts the user to create a task immediately after saving. The task is linked back to the communication event using the existing `LinkedEntityType`/`LinkedEntityId` fields on `TaskItem` (established in F0004). The communication card then displays the linked task with its title and completion status.

## Acceptance Criteria

**Happy Path — Create with Follow-Up:**
- **Given** a distribution user has checked "Create follow-up task" on the Log Communication modal
- **When** they click "Log Communication"
- **Then** the communication event is saved first, then a follow-up task modal appears pre-filled with the entity context

- **When** the user fills the task title, due date, and assignee and clicks "Create Task"
- **Then** a TaskItem is created with `LinkedEntityType = "CommunicationEvent"`, `LinkedEntityId = communicationEvent.Id`, the task is linked to the communication event via `FollowUpTaskId`, and the communication card shows the linked task

**Follow-Up Task Link on Card:**
- **Given** a communication event has a linked follow-up task
- **When** the Communications feed is viewed
- **Then** the card shows "↳ Follow-up: [{task title} →]" as a clickable link that navigates to the Task Center detail view

**Completed Task:**
- **Given** the linked follow-up task has been completed
- **When** the communication card is viewed
- **Then** the task link shows "✓ {task title}" in a completed/muted style

**Skip Follow-Up:**
- **Given** the user checks "Create follow-up task" then clicks "Skip" in the follow-up modal
- **When** the modal closes
- **Then** the communication event is already saved; no task is created; the card shows no follow-up link

**Alternative Flows / Edge Cases:**
- Communication event creation fails → follow-up modal does not appear
- Task creation fails → error shown inside the follow-up modal; communication event already saved; user can retry or skip
- `FollowUpTaskId` already set on an event → creating a second follow-up is not supported in MVP; link is read-only
- User navigates away before completing the follow-up modal → communication event is already saved; no task created

**Checklist:**
- [ ] "Create follow-up task" checkbox visible on Log Communication modal (all event types)
- [ ] Follow-up modal appears after successful communication save when checkbox was checked
- [ ] Follow-up modal pre-fills linked entity context (entity type and display name)
- [ ] TaskItem created with LinkedEntityType=`CommunicationEvent`, LinkedEntityId=communicationEvent.Id
- [ ] CommunicationEvent.FollowUpTaskId set to the new task ID
- [ ] Communication card shows "↳ Follow-up: [{title} →]" when FollowUpTaskId is set
- [ ] Task link navigates to Task Center detail view
- [ ] Completed task shows checkmark + muted style on the card
- [ ] Skipping the follow-up modal leaves the communication saved with no task

## Data Requirements

**Follow-Up Task Create Request (POST /api/communications/{id}/follow-up):**
- `title` (string, required, max 500 chars): Task title
- `dueDate` (date, optional): Task due date
- `assignedToUserId` (uuid, optional): Defaults to the current user
- `priority` (enum, optional): Defaults to `Normal`

**Effect on CommunicationEvent:**
- `FollowUpTaskId` updated to the new TaskItem.Id

**Effect on TaskItem:**
- `LinkedEntityType` = `CommunicationEvent`
- `LinkedEntityId` = communicationEvent.Id
- `Title`, `DueDate`, `AssignedToUserId`, `Priority` set from request

## Role-Based Visibility

**Roles that can create a follow-up task:**
- Distribution User, Underwriter, Distribution Manager, Admin

**Task visibility after creation:**
- Follows existing TaskItem visibility rules from F0004

## Non-Functional Expectations

- Performance: Follow-up task creation completes in < 500ms
- Reliability: If task creation fails, the communication event is not rolled back (it was already committed); failure is surfaced in the follow-up modal

## Dependencies

**Depends On:**
- F0021-S0001 — Communication event must exist before a follow-up task can be linked
- F0004 — TaskItem entity with LinkedEntityType/LinkedEntityId fields (archived)
- F0003 — Task CRUD API (archived)

**Related Stories:**
- F0021-S0004 — Follow-up task link displayed on the communication card in the feed

## Business Rules

1. **Communication Saved First:** The communication event is always persisted before the follow-up modal appears. A failed task creation does not roll back the communication.
2. **One Follow-Up Per Event in MVP:** Each communication event supports a single FollowUpTaskId. Creating a second follow-up is not supported in MVP; the existing link is shown as read-only.
3. **LinkedEntityType Reuse:** The follow-up task uses the existing `TaskItem.LinkedEntityType = "CommunicationEvent"` pattern, consistent with how tasks are linked to submissions, renewals, and other entities in F0004. No new tables or schema changes are required for the task side.
4. **Assignee Defaults to Creator:** If the user does not specify an assignee in the follow-up modal, the task is assigned to the logged-in user.
5. **Skip Is Always Available:** The follow-up modal always offers a "Skip" action. The communication event is already saved; skipping simply closes the modal without creating a task.

## Out of Scope

- Multiple follow-up tasks per communication event (deferred)
- Automatic task creation without user confirmation
- Follow-up reminders or escalation rules
- Linking an existing task to a communication event (create-new only in MVP)

## UI/UX Notes

- Screens involved: Log Communication modal (checkbox), Follow-Up Task modal (appears after save), Communications feed card (task link)
- Key interactions:
  1. User checks "Create follow-up task" before clicking "Log Communication"
  2. Communication is saved; follow-up modal slides in
  3. User fills title, optional due date and assignee, clicks "Create Task" or "Skip"
  4. Feed card shows "↳ Follow-up: [Send renewal proposal →]" in muted link style below body
- The follow-up modal should be lightweight (3–4 fields) to maintain flow momentum

## Questions & Assumptions

**Open Questions:**
- [ ] Should users be able to link an existing task to a communication event after the fact? Deferred — create-new only in MVP.

**Assumptions (to be validated):**
- One follow-up task per communication event is sufficient for MVP
- Communication is saved before the follow-up modal appears (two-step, not atomic)
- Assignee defaults to the current user if left blank

## Definition of Done

- [ ] Acceptance criteria met
- [ ] Follow-up checkbox and modal working end-to-end
- [ ] FollowUpTaskId set on CommunicationEvent after task creation
- [ ] Task card link shown on communication card (with completed state)
- [ ] Skip flow leaves communication saved, no task created
- [ ] Tests pass
- [ ] Story filename matches `Story ID` prefix (`F0021-S0005-...`)
- [ ] Story index regenerated if story file was added/renamed/moved
