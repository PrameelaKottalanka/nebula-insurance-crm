import { useState } from 'react';
import { useCommunicationList } from '../hooks/useCommunications';
import { CommunicationEventCard } from './CommunicationEventCard';
import { LogCommunicationModal } from './LogCommunicationModal';

interface Props {
  entityType: string;
  entityId: string;
}

export function CommunicationFeed({ entityType, entityId }: Props) {
  const [page, setPage] = useState(1);
  const [logOpen, setLogOpen] = useState(false);

  const { data, isLoading, isError } = useCommunicationList(entityType, entityId, page, 25);

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-text-muted">
          {data ? `${data.totalCount} communication${data.totalCount !== 1 ? 's' : ''}` : ''}
        </p>
        <button
          type="button"
          onClick={() => setLogOpen(true)}
          className="rounded-lg bg-nebula-violet px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-nebula-violet/90"
        >
          + Log Communication
        </button>
      </div>

      {/* Content */}
      {isLoading && (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="glass-card rounded-xl p-4 animate-pulse space-y-2">
              <div className="h-3 w-24 rounded bg-surface-border" />
              <div className="h-4 w-3/4 rounded bg-surface-border" />
              <div className="h-3 w-1/2 rounded bg-surface-border" />
            </div>
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-xl border border-red-500/20 bg-red-500/5 p-4 text-sm text-red-400">
          Failed to load communications. Please try again.
        </div>
      )}

      {!isLoading && !isError && data?.data.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-surface-border py-12 text-center">
          <p className="text-sm font-medium text-text-secondary">No communications yet</p>
          <p className="mt-1 text-xs text-text-muted">Log a note, call, or meeting to get started.</p>
          <button
            type="button"
            onClick={() => setLogOpen(true)}
            className="mt-4 rounded-lg bg-nebula-violet px-4 py-2 text-sm font-medium text-white hover:bg-nebula-violet/90"
          >
            Log Communication
          </button>
        </div>
      )}

      {!isLoading && !isError && data && data.data.length > 0 && (
        <div className="space-y-3">
          {data.data.map((event) => (
            <CommunicationEventCard
              key={event.id}
              event={event}
              entityType={entityType}
              entityId={entityId}
            />
          ))}
        </div>
      )}

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between pt-2">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="rounded-lg border border-surface-border bg-surface-card px-3 py-1.5 text-xs font-medium text-text-secondary disabled:opacity-40 hover:bg-surface-card-hover"
          >
            Previous
          </button>
          <span className="text-xs text-text-muted">
            Page {data.page} of {data.totalPages}
          </span>
          <button
            type="button"
            disabled={page >= data.totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="rounded-lg border border-surface-border bg-surface-card px-3 py-1.5 text-xs font-medium text-text-secondary disabled:opacity-40 hover:bg-surface-card-hover"
          >
            Next
          </button>
        </div>
      )}

      <LogCommunicationModal
        open={logOpen}
        onClose={() => setLogOpen(false)}
        entityType={entityType}
        entityId={entityId}
      />
    </div>
  );
}
