import { useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/services/api';
import type {
  CommunicationEventSummaryDto,
  CommunicationLogRequest,
  CommunicationEditRequest,
  CommunicationRedactRequest,
  CommunicationFollowUpRequest,
} from '../types';
import type { TaskSummaryDto } from '@/features/tasks/types';

export function useLogCommunication(entityType: string, entityId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CommunicationLogRequest) =>
      api.post<CommunicationEventSummaryDto>('/communications', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['communications'] });
    },
  });
}

export function useEditCommunication(entityType: string, entityId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: CommunicationEditRequest }) =>
      api.put<CommunicationEventSummaryDto>(`/communications/${id}`, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['communications', entityType, entityId] });
    },
  });
}

export function useRedactCommunication(entityType: string, entityId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: CommunicationRedactRequest }) =>
      api.post<CommunicationEventSummaryDto>(`/communications/${id}/redact`, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['communications', entityType, entityId] });
    },
  });
}

export function useCreateFollowUp(entityType: string, entityId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      communicationId,
      body,
    }: {
      communicationId: string;
      body: CommunicationFollowUpRequest;
    }) => api.post<TaskSummaryDto>(`/communications/${communicationId}/follow-up`, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['communications', entityType, entityId] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['my', 'tasks'] });
    },
  });
}
