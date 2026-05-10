# Story to Suite Mapping — F0021 Communication Hub and Activity Capture

## F0021-S0001 — Log Communication Note

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `LogCommunicationModal.test.tsx`, `CommunicationFeed.test.tsx` | `planning-mds/operations/evidence/F0021/component.log` |
| Integration | `pnpm test:integration` — `BrokerDetailPage.integration.test.tsx` | `planning-mds/operations/evidence/F0021/integration.log` |

## F0021-S0002 — Log Phone Call

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `LogCommunicationModal.test.tsx` (Call tab tests) | `planning-mds/operations/evidence/F0021/component.log` |

## F0021-S0003 — Log Meeting

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `LogCommunicationModal.test.tsx` (Meeting tab tests) | `planning-mds/operations/evidence/F0021/component.log` |

## F0021-S0004 — View Communication Feed

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `CommunicationFeed.test.tsx` | `planning-mds/operations/evidence/F0021/component.log` |
| Integration | `pnpm test:integration` — `BrokerDetailPage.integration.test.tsx` | `planning-mds/operations/evidence/F0021/integration.log` |

## F0021-S0005 — Create Follow-Up Task

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `CommunicationEventCard.test.tsx` (Follow-up modal tests) | `planning-mds/operations/evidence/F0021/component.log` |

## F0021-S0006 — Edit and Redact

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `CommunicationEventCard.test.tsx` (Edit modal, Redact modal, visibility tests) | `planning-mds/operations/evidence/F0021/component.log` |

## F0021-S0007 — Filter Communication Feed

| Layer | Command / Suite | Evidence |
|-------|------------------|----------|
| Component | `pnpm test` — `CommunicationFeed.test.tsx` (filter tests) | `planning-mds/operations/evidence/F0021/component.log` |

## Coverage

| Layer | Command | Evidence |
|-------|---------|----------|
| Coverage | `pnpm test:coverage` | `planning-mds/operations/evidence/F0021/coverage.log`, `experience/coverage/lcov.info`, `experience/coverage/coverage-summary.json` |
