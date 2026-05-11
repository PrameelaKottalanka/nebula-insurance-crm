# F0021 — AI Prompt Series

This document contains the professional prompt for each commit on the `feat/F0021-communication-hub-and-activity-capture` branch. Each prompt is self-contained and, when given to an AI agent, will reproduce the exact work done in the corresponding commit. The prompts are sequential — each one builds on the outputs of the previous step.

> **Purpose:** Technical assessment reference for AI agentic prompting skills.
> **Branch:** `feat/F0021-communication-hub-and-activity-capture`
> **Author:** Prameela Kottalanka

---

## Prompt 1 — Product Manager: Story files and planning docs
**Commit:** `7de5918`

You are acting as the **product-manager** agent for the Nebula Insurance CRM (`planning-mds/` is your working directory).

Create the full planning documentation for feature **F0021 — Communication Hub & Activity Capture** under `planning-mds/features/F0021-communication-hub-and-activity-capture/`.

Deliverables:
1. **PRD.md** — Feature statement, business objective, problem statement, scope/boundaries (in scope: notes/calls/meetings on broker, account, submission, policy, renewal records; follow-up creation; out of scope: full email client, marketing automation), success criteria, risks/assumptions, dependencies on F0016 and F0004, stories list S0001–S0007.
2. **README.md** — One-page orientation: what the feature does, which stories exist, who the primary users are, and a link to the PRD and STATUS.
3. **STATUS.md** — Story checklist for S0001–S0007 (all `Not Started`), backend and frontend progress sections, cross-cutting items (migration, Bruno collection, no TODOs), and a required-signoff table (Quality Engineer and Code Reviewer are required; Security/DevOps/Architect are TBD).
4. **GETTING-STARTED.md** — Prerequisites, services to run (`docker compose up -d db authentik`, then `dotnet run` and `pnpm dev`), key files table (entity, enums, EF config, repository, service, endpoints, frontend feature, reference patterns), environment variables (none new), and a numbered how-to-verify checklist covering all five entity types.
5. **Seven story files** — One per story, following the existing user-story template (`template: user-story`, `version: 1.1`, `applies_to: product-manager`):
   - `F0021-S0001-log-communication-note.md` — Log a free-text Note against any entity; establishes the core `CommunicationEvent` model.
   - `F0021-S0002-log-phone-call.md` — Log a Call with direction (Inbound/Outbound) and duration in minutes.
   - `F0021-S0003-log-meeting.md` — Log a Meeting with meeting type (Internal/External/BrokerFacing) and duration.
   - `F0021-S0004-view-communication-feed.md` — Paginated communications tab on entity detail pages, sorted newest first with month grouping.
   - `F0021-S0005-create-follow-up-task.md` — Create a follow-up TaskItem linked to a CommunicationEvent at log time.
   - `F0021-S0006-edit-and-redact.md` — Author can edit within 24 hours; Admin can redact at any time (replaces body with placeholder).
   - `F0021-S0007-filter-communication-feed.md` — Filter feed by event type and date range. **Mark this story as Phase 1 / post-MVP (deferred).**

   Each story must include: user story statement, context/background, full acceptance criteria (happy path + edge cases with HTTP status codes and ProblemDetails error codes), and an AC checklist.

6. Update `planning-mds/features/ROADMAP.md` — Add F0021 to the CRM Release MVP section.
7. Update `planning-mds/features/STORY-INDEX.md` — Add all seven stories with ID, title, feature, priority, and phase columns.

Follow the conventions of existing features (e.g. F0016, F0004) for file structure and frontmatter format. Do not invent technical architecture — that is the architect's responsibility.

---

## Prompt 2 — Architect: Assembly plan, API contract, JSON schemas
**Commit:** `5763ba4`

You are acting as the **architect** agent for the Nebula Insurance CRM. The product-manager has completed F0021 story files in `planning-mds/features/F0021-communication-hub-and-activity-capture/`. Read all seven story files and the PRD before producing your deliverables.

Produce three sets of artifacts:

**1. Feature Assembly Plan** — `planning-mds/features/F0021-communication-hub-and-activity-capture/feature-assembly-plan.md`

This is the implementation-ready specification that backend and frontend developers execute. It must include:
- Build order table (Domain+Infrastructure first → Application+API log/list → Application+API edit/redact/follow-up → Frontend)
- Existing files that must be modified: `AppDbContext.cs`, `DependencyInjection.cs`, `Program.cs`, `nebula-api.yaml`
- New files table for every layer (Domain entity + constants, Infrastructure config + repository + migration, Application interface + DTOs + validators + service, API endpoints, Frontend types + hooks + components)
- Full `CommunicationEvent` entity field specification: `Id`, `EventType` (Note/Call/Meeting), `PrimaryEntityType`, `PrimaryEntityId`, `Body` (max 4000 chars), `OccurredAt`, `AuthoredByUserId`, `AuthoredByDisplayName`, `Direction` (nullable, Call only), `DurationMinutes` (nullable), `MeetingType` (nullable), `Outcome` (nullable), `IsRedacted`, `RedactedAt`, `RedactedByUserId`, `CreatedAt`, `UpdatedAt`, `xmin` (EF concurrency token)
- Repository contract: `ListAsync`, `GetByIdAsync`, `AddAsync`, `EntityExistsAsync`
- Service method logic flows with guard conditions and error codes for: `Log`, `List`, `GetById`, `Edit` (24-hour author-only window, error codes: `edit_window_expired`, `not_author`, `event_redacted`, `no_changes`), `Redact` (Admin role only, error codes: `already_redacted`), `CreateFollowUp` (error code: `follow_up_exists`)
- Per-endpoint Casbin enforcement table: `communication:create` (all internal roles), `communication:read` (all internal roles), `communication:edit` (all internal roles), `communication:redact` (Admin only); BrokerUser gets HTTP 403 on all routes
- HTTP response tables for all 6 endpoints (success codes, error codes, ProblemDetails error strings)
- ActivityTimelineEvent spec: append `CommunicationLogged` on log, `CommunicationEdited` on edit, `CommunicationRedacted` on redact
- Casbin policy rows to add to `planning-mds/security/policies/policy.csv`
- DI registration changes
- Explicit note: S0007 is post-MVP, not in scope for this plan

**2. JSON Schema files** — Six Draft-07 schema files under `planning-mds/features/F0021-communication-hub-and-activity-capture/schemas/`:
- `communication-log-request.schema.json` — required: Body, PrimaryEntityType, PrimaryEntityId; optional: OccurredAt, Direction, DurationMinutes, MeetingType, Outcome, CreateFollowUp
- `communication-edit-request.schema.json` — required: Body
- `communication-redact-request.schema.json` — required: Reason
- `communication-follow-up-request.schema.json` — required: Title, DueDate
- `communication-event-summary.schema.json` — full read response shape
- `paginated-communication-list.schema.json` — wrapper with items array, totalCount, page, pageSize

**3. OpenAPI patch** — `planning-mds/api/nebula-api-f0021-patch.yaml`

OpenAPI 3.0.3 fragment ready to merge into `nebula-api.yaml`. Must define:
- `Communications` tag
- 6 paths under `/{entityType}/{entityId}/communications`: POST (log), GET (list), GET `/{eventId}` (detail), PATCH `/{eventId}` (edit), POST `/{eventId}/redact`, POST `/{eventId}/follow-up-task`
- 6 component schemas matching the JSON Schema files above
- All paths include `bearerAuth` security and appropriate 400/403/404/409 responses with RFC 7807 ProblemDetails

Reference existing endpoint patterns in `engine/src/Nebula.Api/Endpoints/SubmissionEndpoints.cs` for route conventions. Reference `ActivityTimelineEvent.cs` for the append-only audit pattern.

---

## Prompt 3 — Backend Developer: Entity, service, endpoints, migration
**Commit:** `692f1eb`

You are acting as the **backend-developer** agent for the Nebula Insurance CRM. Read the following before writing any code:
- `planning-mds/features/F0021-communication-hub-and-activity-capture/feature-assembly-plan.md` (your execution spec)
- `planning-mds/features/F0021-communication-hub-and-activity-capture/F0021-S0001` through `S0006` story files (acceptance criteria)
- `planning-mds/api/nebula-api-f0021-patch.yaml` (API contract)
- `engine/src/Nebula.Api/Endpoints/SubmissionEndpoints.cs` (routing pattern to follow)
- `engine/src/Nebula.Domain/Entities/ActivityTimelineEvent.cs` (append-only audit pattern)
- `engine/src/Nebula.Infrastructure/Persistence/AppDbContext.cs` (existing DbSets)

Implement the full backend for F0021 (stories S0001–S0006). Create all files listed in the assembly plan's "New Files" table and modify all files in the "Existing Code" table:

**Non-negotiables:**
- Every mutation (Log, Edit, Redact) must append an immutable `ActivityTimelineEvent` to the linked entity's timeline
- All endpoints must enforce Casbin ABAC via `IAuthorizationService` — no endpoint is reachable without the appropriate `communication:*` policy check
- BrokerUser role must receive HTTP 403 on all communication routes
- Edit is restricted to the original author within a 24-hour window (`edit_window_expired` / `not_author` error codes)
- Redact is restricted to Admin role only (`already_redacted` error code if already redacted)
- `CreateFollowUp` must reject if a follow-up task already exists (`follow_up_exists` error code)
- FluentValidation for all request types — conditional rules: Direction is required for Call events; DurationMinutes is required for Call and Meeting events
- Add 6 new error codes to `ProblemDetailsHelper.cs`: `event_redacted`, `already_redacted`, `not_author`, `edit_window_expired`, `follow_up_exists`, `no_changes`
- Add Casbin policy rows to `planning-mds/security/policies/policy.csv` per the assembly plan
- Run `dotnet ef migrations add F0021_AddCommunicationEvents` to generate the migration — include the generated migration file
- Build must pass with 0 errors. S0007 (filter) is post-MVP — do not implement it

Follow the clean architecture layering strictly: no EF or infrastructure references in Domain or Application layers. Match response shapes to `nebula-api-f0021-patch.yaml` exactly.

---

## Prompt 4 — Quality Engineer: Backend unit and integration tests
**Commit:** `60fcfd3`

You are acting as the **quality-engineer** agent for the Nebula Insurance CRM. The backend for F0021 has been implemented. Read the following before writing tests:
- All 6 story files (S0001–S0006) for acceptance criteria
- `engine/src/Nebula.Application/Services/CommunicationEventService.cs`
- `engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs`
- Existing test files under `engine/tests/` for conventions (stub pattern for unit tests, Testcontainers for integration)

Write 60 tests across two files:

**1. Unit tests** — `engine/tests/Nebula.Application.Tests/Unit/CommunicationEventServiceTests.cs`
- Use the stub/mock pattern (no database); mock `ICommunicationEventRepository` and `IActivityTimelineService`
- Cover all 6 service methods: `Log`, `List`, `GetById`, `Edit`, `Redact`, `CreateFollowUp`
- Test every guard condition and error code from the acceptance criteria: `edit_window_expired`, `not_author`, `event_redacted`, `already_redacted`, `follow_up_exists`, `no_changes`, `entity_not_found`
- Test happy path and all documented edge cases per story

**2. Integration tests** — `engine/tests/Nebula.Api.Tests/Integration/CommunicationEndpointTests.cs`
- Use Testcontainers with a real PostgreSQL container (follow existing integration test base class conventions)
- Test all 6 routes: POST /log, GET /list, GET /detail, PATCH /edit, POST /redact, POST /follow-up-task
- Assert HTTP status codes, response body shapes, and ProblemDetails error codes for failure cases
- Test Casbin enforcement: verify BrokerUser receives 403 on all routes; verify `communication:redact` requires Admin role

After writing tests, update `planning-mds/features/F0021-communication-hub-and-activity-capture/STATUS.md` to reflect backend unit and integration tests as written (note they require Docker to run).

---

## Prompt 5 — Frontend Developer: Components, hooks, page wiring
**Commit:** `8842f57`

You are acting as the **frontend-developer** agent for the Nebula Insurance CRM. Read the following before writing any code:
- `planning-mds/features/F0021-communication-hub-and-activity-capture/feature-assembly-plan.md` (frontend section)
- Story files S0001–S0006 (acceptance criteria for UI behaviour)
- `planning-mds/api/nebula-api-f0021-patch.yaml` (API shapes to type against)
- Existing feature modules under `experience/src/features/` for conventions
- Existing detail pages: `AccountDetailPage.tsx`, `BrokerDetailPage.tsx`, `SubmissionDetailPage.tsx`, `PolicyDetailPage.tsx`, `RenewalDetailPage.tsx`

Implement the full communications feature module under `experience/src/features/communications/`:

**Types and hooks:**
- `types.ts` — TypeScript interfaces for `CommunicationEventSummary`, `LogCommunicationRequest`, `EditCommunicationRequest`, `RedactCommunicationRequest`, `CreateFollowUpRequest`, `PaginatedCommunicationList` — derived from the OpenAPI patch
- `hooks/useCommunications.ts` — TanStack Query `useQuery` hook for `GET /{entityType}/{entityId}/communications`
- `hooks/useCommunicationMutations.ts` — `useMutation` hooks for log, edit, redact, and follow-up; each mutation must invalidate the `['communications', entityType, entityId]` query key on success
- `index.ts` — Re-export public surface

**Components:**
- `CommunicationFeed.tsx` — Paginated list grouped by month, empty state when no events, loading skeleton, error state; renders `CommunicationEventCard` per item
- `CommunicationEventCard.tsx` — Three visual variants: Note, Call (with direction badge and duration), Meeting (with type badge and duration); redacted state (body replaced with placeholder, Outcome hidden); "Edited {timestamp}" label when edited; inline Redact button (Admin only) and Edit button (author within 24h); inline follow-up creation via `CreateFollowUpModal`
- `LogCommunicationModal.tsx` — Tabbed modal with Note, Call, and Meeting tabs; conditional fields per tab (direction + duration for Call; meeting type + duration for Meeting); follow-up task checkbox that expands follow-up fields inline; uses shadcn/ui `Dialog`, `Tabs`, `Input`, `Textarea`, `Select`, `Button`

**Non-negotiables:**
- Use semantic Tailwind token classes only — no raw palette classes (`text-red-500` etc.)
- All interactive elements must be keyboard accessible; all form inputs must have associated labels (WCAG 2.1 AA)
- TypeScript strict mode — no `any`, no non-null assertions without justification
- Use `shadcn/ui` components; do not build custom primitives where a shadcn component exists

**Page wiring:**
Add a Communications tab/section to all five entity detail pages: `AccountDetailPage.tsx`, `BrokerDetailPage.tsx`, `SubmissionDetailPage.tsx`, `PolicyDetailPage.tsx`, `RenewalDetailPage.tsx`. Pass the appropriate `entityType` string and `entityId` to `CommunicationFeed` and `LogCommunicationModal`.

---

## Prompt 6 — Quality Engineer: Frontend component and integration tests
**Commit:** `d1c8d2d`

You are acting as the **quality-engineer** agent for the Nebula Insurance CRM. The frontend for F0021 has been implemented. Read the following before writing tests:
- `experience/src/features/communications/components/CommunicationEventCard.tsx`
- `experience/src/features/communications/components/CommunicationFeed.tsx`
- `experience/src/features/communications/components/LogCommunicationModal.tsx`
- Story files S0001–S0006 for acceptance criteria
- Existing test files under `experience/src/` for Vitest + React Testing Library conventions
- Existing MSW mock setup under `experience/src/mocks/`

Write the following:

**1. MSW mock infrastructure** (if not already present):
- `experience/src/mocks/communications.ts` — Factory functions for `CommunicationEventSummary` mock data (Note, Call, Meeting variants; redacted variant; edited variant)
- `experience/src/mocks/handlers.ts` — MSW request handlers for all 6 communication endpoints
- `experience/src/mocks/data.ts` — Seed data additions for communication events

**2. Component tests** using Vitest + React Testing Library + jest-axe:
- `experience/src/features/communications/tests/CommunicationEventCard.test.tsx`
  - Renders Note, Call, Meeting variants correctly
  - Redacted state hides body and Outcome, shows placeholder
  - "Edited" label visible when `editedAt` is set
  - Edit button visible only to author within 24h; Redact button visible only to Admin
  - Follow-up creation flow renders and submits
  - Accessibility: `jest-axe` violation check on all variants

- `experience/src/features/communications/tests/CommunicationFeed.test.tsx`
  - Empty state renders when no events
  - Month grouping renders correctly for events across multiple months
  - Pagination controls appear when `totalCount` exceeds page size
  - Accessibility: `jest-axe` violation check

- `experience/src/features/communications/tests/LogCommunicationModal.test.tsx`
  - Opens and closes correctly
  - Tab switching shows/hides conditional fields (direction and duration for Call; meeting type for Meeting)
  - Form submits with correct payload for each tab type
  - Follow-up checkbox expands follow-up fields
  - Validation errors display for required fields
  - Accessibility: `jest-axe` violation check

---

## Prompt 7 — Architect: Merge OpenAPI patch and sync knowledge graph
**Commit:** `62b1ed1`

You are acting as the **architect** agent for the Nebula Insurance CRM. Implementation of F0021 (backend + frontend + tests) is complete. Perform the post-implementation contract and knowledge graph synchronisation:

**1. Merge OpenAPI patch into main contract**
Read `planning-mds/api/nebula-api-f0021-patch.yaml` (the patch created during Phase B) and merge it into `planning-mds/api/nebula-api.yaml`:
- Add the `Communications` tag to the tags section
- Add all 6 endpoint paths under `/{entityType}/{entityId}/communications`
- Add all 6 component schemas: `CommunicationLogRequest`, `CommunicationEditRequest`, `CommunicationRedactRequest`, `CommunicationFollowUpRequest`, `CommunicationEventSummary`, `PaginatedCommunicationList`
- Verify the merged file is valid OpenAPI 3.0.3 (no duplicate keys, all `$ref` targets resolve)

**2. Register the new entity in the knowledge graph**
- `planning-mds/knowledge-graph/canonical-nodes.yaml` — Add node `entity:communication-event` with label `CommunicationEvent`, type `entity`, and relevant domain metadata
- `planning-mds/knowledge-graph/code-index.yaml` — Add bindings for `entity:communication-event` mapping to:
  - `domain`: `engine/src/Nebula.Domain/Entities/CommunicationEvent.cs`
  - `application`: `engine/src/Nebula.Application/Services/CommunicationEventService.cs`
  - `infrastructure`: `engine/src/Nebula.Infrastructure/Persistence/Repositories/CommunicationEventRepository.cs`
  - `api`: `engine/src/Nebula.Api/Endpoints/CommunicationEndpoints.cs`
  - `tests`: `engine/tests/Nebula.Application.Tests/Unit/CommunicationEventServiceTests.cs`, `engine/tests/Nebula.Api.Tests/Integration/CommunicationEndpointTests.cs`
  - `frontend`: `experience/src/features/communications/`

**3. Regenerate coverage report**
Run the knowledge graph validator script to regenerate `planning-mds/knowledge-graph/coverage-report.yaml` and verify all F0021 bindings appear in the output without errors.

---

## Prompt 8 — Code Reviewer + Quality Engineer: Fix test failures and assemble evidence package
**Commit:** `331ed36`

You are acting as the **code-reviewer** and **quality-engineer** agent for the Nebula Insurance CRM. The F0021 frontend test suite is failing. Fix the following specific failures and then assemble the quality evidence package:

**Fix 1 — `Modal.tsx` focus-stealing (`experience/src/components/ui/Modal.tsx`)**
The `handleClose` function is recreated on every render, causing `useEffect` cleanup/re-registration that steals focus during `user.type` and `user.click` in tests. Fix by storing the `onClose` callback in a ref (`onCloseRef`) and defining `handleClose` with `useCallback` referencing `onCloseRef.current` instead. This prevents the `useEffect` from re-running when `onClose` identity changes.

**Fix 2 — Native `required` attributes blocking jsdom form submission (`CommunicationEventCard.tsx`)**
Remove native HTML `required` attributes from form fields in Edit, Redact, and FollowUp forms inside `CommunicationEventCard.tsx`. jsdom's form validation runs before React's `onSubmit` handler fires, blocking test submissions. Validation is already handled by React state — the native attribute is redundant and breaks the test environment.

**Fix 3 — Duplicate Redact button in DOM**
When the redact modal is open, the card's Redact button remains visible, causing `getByRole('button', { name: /redact/i })` to match two elements in tests. Hide the card-level Redact button when the redact modal is open (conditional render on modal open state).

**Fix 4 — Mutation query invalidation scope (`hooks/useCommunicationMutations.ts`)**
The `invalidateQueries` call after mutations uses a key of `['communications', entityType, entityId]`. Broaden it to `['communications']` so all list queries for this entity type are invalidated after any mutation, not just the exact page currently loaded.

**Fix 5 — Cross-realm `Blob` instanceof check (`experience/src/services/api.test.ts`)**
The `api.test.ts` file uses `instanceof Blob` to assert file upload behaviour. In jsdom, `Blob` from `FormData` is a different realm than the `Blob` global, making `instanceof` return false. Replace with a duck-type check (`blob.size !== undefined && blob.type !== undefined`) or use `Object.prototype.toString.call(blob) === '[object Blob]'`.

**Fix 6 — Test timeout flakiness (`experience/vite.config.ts`)**
Raise the `testTimeout` in vitest config from the default 5000ms to 15000ms to prevent resource-contention failures when the full test suite runs in parallel.

**Fix 7 — Windows path separator bug (`planning-mds/testing/validate-frontend-quality-gate.py`)**
The coverage artifact path check constructs file paths using string concatenation, which produces backslashes on Windows. Change path construction to use `.as_posix()` on `Path` objects so the check passes on both Windows and POSIX systems.

**After all fixes, assemble the F0021 quality evidence package** under `planning-mds/operations/evidence/F0021/`:
- `action-context.md` — Run metadata: feature ID, action, operator, role order, lifecycle stage, inputs used, outcome summary
- `artifact-trace.md` — Maps each story (S0001–S0006) to the implementing files, test files, and coverage entries
- `story-to-suite.md` — Maps each AC from each story to the specific test case that covers it
- `gate-decisions.md` — Records quality gate pass/fail decisions with rationale (include coverage exception: target 80%, actual ~70%, with justification that all new F0021 code paths are directly exercised)
- `lifecycle-gates.log` — Gate activation log entries
- `component.log`, `integration.log`, `accessibility.log`, `coverage.log`, `visual.log` — Test run output logs
- `commands.log` — Exact commands run to produce the evidence
- `artifacts/playwright-report/index.html` — Playwright visual report stub
- `artifacts/visual-test-results/.last-run.json` — Visual test last-run metadata

Update `planning-mds/operations/evidence/frontend-quality/latest-run.json` to point to the F0021 run.
