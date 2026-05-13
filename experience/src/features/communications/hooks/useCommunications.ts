import { useQuery } from '@tanstack/react-query';
import { api } from '@/services/api';
import type { CommunicationEventSummaryDto, PaginatedCommunicationListDto } from '../types';

export function useCommunicationList(
  entityType: string,
  entityId: string,
  page = 1,
  pageSize = 25,
) {
  return useQuery({
    queryKey: ['communications', entityType, entityId, page, pageSize],
    queryFn: () =>
      api.get<PaginatedCommunicationListDto>(
        `/communications?entityType=${entityType}&entityId=${entityId}&page=${page}&pageSize=${pageSize}`,
      ),
    enabled: !!entityId,
  });
}

export function useCommunication(id: string) {
  return useQuery({
    queryKey: ['communications', id],
    queryFn: () => api.get<CommunicationEventSummaryDto>(`/communications/${id}`),
    enabled: !!id,
  });
}
