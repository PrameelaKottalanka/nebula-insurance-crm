export type CommunicationEventType = 'Note' | 'Call' | 'Meeting';
export type CommunicationDirection = 'Inbound' | 'Outbound';
export type CommunicationEntityType = 'Broker' | 'Account' | 'Submission' | 'Policy' | 'Renewal';

export interface CommunicationEventSummaryDto {
  id: string;
  eventType: CommunicationEventType;
  direction: CommunicationDirection | null;
  durationMinutes: number | null;
  subject: string | null;
  body: string | null;
  outcome: string | null;
  occurredAt: string;
  primaryEntityType: string;
  primaryEntityId: string;
  authoredByUserId: string;
  authoredByDisplayName: string;
  lastEditedAt: string | null;
  isRedacted: boolean;
  redactedByDisplayName: string | null;
  redactedAt: string | null;
  redactionReason: string | null;
  followUpTaskId: string | null;
  followUpTaskTitle: string | null;
  followUpTaskStatus: string | null;
}

export interface PaginatedCommunicationListDto {
  data: CommunicationEventSummaryDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CommunicationLogRequest {
  eventType: CommunicationEventType;
  direction?: CommunicationDirection;
  durationMinutes?: number;
  subject?: string;
  body: string;
  outcome?: string;
  occurredAt?: string;
  primaryEntityType: string;
  primaryEntityId: string;
}

export interface CommunicationEditRequest {
  body?: string;
  subject?: string;
  outcome?: string;
  occurredAt?: string;
}

export interface CommunicationRedactRequest {
  reason: string;
}

export interface CommunicationFollowUpRequest {
  title: string;
  dueDate?: string;
  assignedToUserId?: string;
  priority?: string;
}
