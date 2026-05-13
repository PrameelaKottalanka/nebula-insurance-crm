import type {
  CommunicationEventSummaryDto,
  CommunicationLogRequest,
  CommunicationEditRequest,
  CommunicationRedactRequest,
} from '@/features/communications/types'
import type { TaskSummaryDto } from '@/features/tasks/types'

let _events: CommunicationEventSummaryDto[] = []
let _nextTaskId = 1

export function resetCommunicationMockState() {
  _events = []
  _nextTaskId = 1
}

export function seedCommunication(
  partial: Partial<CommunicationEventSummaryDto> & { id: string },
): CommunicationEventSummaryDto {
  const event: CommunicationEventSummaryDto = {
    eventType: 'Note',
    direction: null,
    durationMinutes: null,
    subject: null,
    body: 'Default body',
    outcome: null,
    occurredAt: new Date().toISOString(),
    primaryEntityType: 'Broker',
    primaryEntityId: 'broker-1',
    authoredByUserId: 'user-author',
    authoredByDisplayName: 'Test Author',
    lastEditedAt: null,
    isRedacted: false,
    redactedByDisplayName: null,
    redactedAt: null,
    redactionReason: null,
    followUpTaskId: null,
    followUpTaskTitle: null,
    followUpTaskStatus: null,
    ...partial,
  }
  _events.push(event)
  return event
}

export function listCommunications(params: URLSearchParams): {
  data: CommunicationEventSummaryDto[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
} {
  const entityType = params.get('entityType')
  const entityId = params.get('entityId')
  const page = Number(params.get('page') ?? '1')
  const pageSize = Number(params.get('pageSize') ?? '25')

  const filtered = _events.filter(
    (e) => e.primaryEntityType === entityType && e.primaryEntityId === entityId,
  )
  const totalCount = filtered.length
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))
  const start = (page - 1) * pageSize
  const data = filtered.slice(start, start + pageSize)

  return { data, page, pageSize, totalCount, totalPages }
}

export function getCommunication(id: string): CommunicationEventSummaryDto | undefined {
  return _events.find((e) => e.id === id)
}

export function createCommunication(body: CommunicationLogRequest): CommunicationEventSummaryDto {
  const event: CommunicationEventSummaryDto = {
    id: `comm-${Date.now()}`,
    eventType: body.eventType,
    direction: body.direction ?? null,
    durationMinutes: body.durationMinutes ?? null,
    subject: body.subject ?? null,
    body: body.body,
    outcome: body.outcome ?? null,
    occurredAt: body.occurredAt ?? new Date().toISOString(),
    primaryEntityType: body.primaryEntityType,
    primaryEntityId: body.primaryEntityId,
    authoredByUserId: 'user-author',
    authoredByDisplayName: 'Test Author',
    lastEditedAt: null,
    isRedacted: false,
    redactedByDisplayName: null,
    redactedAt: null,
    redactionReason: null,
    followUpTaskId: null,
    followUpTaskTitle: null,
    followUpTaskStatus: null,
  }
  _events.push(event)
  return event
}

export function editCommunication(
  id: string,
  body: CommunicationEditRequest,
): CommunicationEventSummaryDto | null {
  const idx = _events.findIndex((e) => e.id === id)
  if (idx === -1) return null
  _events[idx] = {
    ..._events[idx],
    body: body.body ?? _events[idx].body,
    subject: body.subject !== undefined ? body.subject : _events[idx].subject,
    outcome: body.outcome !== undefined ? body.outcome : _events[idx].outcome,
    lastEditedAt: new Date().toISOString(),
  }
  return _events[idx]
}

export function redactCommunication(
  id: string,
  body: CommunicationRedactRequest,
): CommunicationEventSummaryDto | null {
  const idx = _events.findIndex((e) => e.id === id)
  if (idx === -1 || _events[idx].isRedacted) return null
  _events[idx] = {
    ..._events[idx],
    isRedacted: true,
    body: null,
    outcome: null,
    redactedByDisplayName: 'Admin User',
    redactedAt: new Date().toISOString(),
    redactionReason: body.reason,
  }
  return _events[idx]
}

export function createFollowUp(communicationId: string, title: string): TaskSummaryDto | null {
  const idx = _events.findIndex((e) => e.id === communicationId)
  if (idx === -1) return null
  const taskId = `task-comm-${_nextTaskId++}`
  _events[idx] = { ..._events[idx], followUpTaskId: taskId, followUpTaskTitle: title }
  return {
    id: taskId,
    title,
    status: 'Open',
    dueDate: null,
    linkedEntityType: 'CommunicationEvent',
    linkedEntityId: communicationId,
    linkedEntityName: 'Communication',
    isOverdue: false,
  }
}
