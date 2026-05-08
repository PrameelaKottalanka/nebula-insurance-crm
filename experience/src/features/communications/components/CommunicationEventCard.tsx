import { useState } from 'react';
import { Modal } from '@/components/ui/Modal';
import { TextInput } from '@/components/ui/TextInput';
import { useCurrentUser } from '@/features/auth';
import { useEditCommunication, useRedactCommunication, useCreateFollowUp } from '../hooks/useCommunicationMutations';
import type { CommunicationEventSummaryDto } from '../types';

interface Props {
  event: CommunicationEventSummaryDto;
  entityType: string;
  entityId: string;
}

const EVENT_ICONS: Record<string, string> = {
  Note: '📝',
  Call: '📞',
  Meeting: '🤝',
};

const DIRECTION_LABEL: Record<string, string> = {
  Inbound: 'Inbound',
  Outbound: 'Outbound',
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  });
}

const EDIT_WINDOW_MS = 24 * 60 * 60 * 1000;

export function CommunicationEventCard({ event, entityType, entityId }: Props) {
  const currentUser = useCurrentUser();
  const isAdmin = currentUser?.roles.includes('Admin') ?? false;
  const isAuthor = currentUser?.sub === event.authoredByUserId;
  const withinEditWindow =
    Date.now() - new Date(event.occurredAt).getTime() < EDIT_WINDOW_MS;
  const canEdit = !event.isRedacted && (isAdmin || (isAuthor && withinEditWindow));
  const canRedact = isAdmin && !event.isRedacted;

  const [editOpen, setEditOpen] = useState(false);
  const [redactOpen, setRedactOpen] = useState(false);
  const [followUpOpen, setFollowUpOpen] = useState(false);

  const [editBody, setEditBody] = useState('');
  const [editSubject, setEditSubject] = useState('');
  const [editOutcome, setEditOutcome] = useState('');
  const [editError, setEditError] = useState('');

  const [redactReason, setRedactReason] = useState('');
  const [redactError, setRedactError] = useState('');

  const [followUpTitle, setFollowUpTitle] = useState('');
  const [followUpError, setFollowUpError] = useState('');

  const editMutation = useEditCommunication(entityType, entityId);
  const redactMutation = useRedactCommunication(entityType, entityId);
  const followUpMutation = useCreateFollowUp(entityType, entityId);

  function openEdit() {
    setEditBody(event.body ?? '');
    setEditSubject(event.subject ?? '');
    setEditOutcome(event.outcome ?? '');
    setEditError('');
    setEditOpen(true);
  }

  function handleEdit(e: React.FormEvent) {
    e.preventDefault();
    if (!editBody.trim()) { setEditError('Body is required.'); return; }
    editMutation.mutate(
      { id: event.id, body: { body: editBody.trim(), subject: editSubject.trim() || undefined, outcome: editOutcome.trim() || undefined } },
      { onSuccess: () => setEditOpen(false), onError: () => setEditError('Failed to save. Please try again.') },
    );
  }

  function handleRedact(e: React.FormEvent) {
    e.preventDefault();
    if (!redactReason.trim()) { setRedactError('Reason is required.'); return; }
    redactMutation.mutate(
      { id: event.id, body: { reason: redactReason.trim() } },
      { onSuccess: () => { setRedactOpen(false); setRedactReason(''); }, onError: () => setRedactError('Failed to redact. Please try again.') },
    );
  }

  function handleFollowUp(e: React.FormEvent) {
    e.preventDefault();
    if (!followUpTitle.trim()) { setFollowUpError('Title is required.'); return; }
    followUpMutation.mutate(
      { communicationId: event.id, body: { title: followUpTitle.trim() } },
      { onSuccess: () => { setFollowUpOpen(false); setFollowUpTitle(''); }, onError: () => setFollowUpError('Follow-up already exists or request failed.') },
    );
  }

  return (
    <>
      <div className="glass-card rounded-xl p-4 space-y-2">
        {/* Header */}
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="text-base">{EVENT_ICONS[event.eventType] ?? '💬'}</span>
            <span className="rounded bg-surface-main/55 px-1.5 py-0.5 text-xs font-medium uppercase tracking-wide text-text-muted">
              {event.eventType}
            </span>
            {event.direction && (
              <span className="text-xs text-text-muted">
                {DIRECTION_LABEL[event.direction]}
              </span>
            )}
            {event.durationMinutes != null && (
              <span className="text-xs text-text-muted">{event.durationMinutes} min</span>
            )}
            {event.isRedacted && (
              <span className="rounded bg-red-500/15 px-1.5 py-0.5 text-xs font-medium text-red-400">
                Redacted
              </span>
            )}
            {event.lastEditedAt && !event.isRedacted && (
              <span className="text-xs text-text-muted italic">edited</span>
            )}
          </div>
          <span className="shrink-0 text-xs text-text-muted">{formatDate(event.occurredAt)}</span>
        </div>

        {/* Subject */}
        {event.subject && !event.isRedacted && (
          <p className="text-sm font-medium text-text-primary">{event.subject}</p>
        )}

        {/* Body */}
        {event.isRedacted ? (
          <p className="text-sm italic text-text-muted">
            Redacted by {event.redactedByDisplayName} on {event.redactedAt ? formatDate(event.redactedAt) : '—'}
          </p>
        ) : (
          <p className="text-sm text-text-secondary whitespace-pre-wrap">{event.body}</p>
        )}

        {/* Outcome */}
        {event.outcome && !event.isRedacted && (
          <p className="text-xs text-text-muted">
            <span className="font-medium">Outcome:</span> {event.outcome}
          </p>
        )}

        {/* Footer */}
        <div className="flex items-center justify-between gap-2 pt-1 border-t border-surface-border/50">
          <div className="flex items-center gap-2">
            <span className="text-xs text-text-muted">{event.authoredByDisplayName}</span>
            {event.followUpTaskId && (
              <span className="rounded bg-nebula-violet/15 px-1.5 py-0.5 text-xs font-medium text-nebula-violet">
                Follow-up: {event.followUpTaskTitle ?? 'Task'}
              </span>
            )}
          </div>
          <div className="flex items-center gap-1">
            {canEdit && (
              <button
                type="button"
                onClick={openEdit}
                className="rounded px-2 py-0.5 text-xs text-text-muted transition-colors hover:bg-surface-card-hover hover:text-text-primary"
              >
                Edit
              </button>
            )}
            {canRedact && (
              <button
                type="button"
                onClick={() => { setRedactReason(''); setRedactError(''); setRedactOpen(true); }}
                className="rounded px-2 py-0.5 text-xs text-red-400 transition-colors hover:bg-red-500/10"
              >
                Redact
              </button>
            )}
            {!event.followUpTaskId && !event.isRedacted && (
              <button
                type="button"
                onClick={() => { setFollowUpTitle(''); setFollowUpError(''); setFollowUpOpen(true); }}
                className="rounded px-2 py-0.5 text-xs text-text-muted transition-colors hover:bg-surface-card-hover hover:text-text-primary"
              >
                + Follow-up
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Edit Modal */}
      <Modal open={editOpen} onClose={() => setEditOpen(false)} title="Edit Communication">
        <form onSubmit={handleEdit} className="space-y-3">
          <TextInput
            label="Subject"
            value={editSubject}
            onChange={(e) => setEditSubject(e.target.value)}
            placeholder="Optional subject..."
          />
          <div className="space-y-1.5">
            <label className="block text-xs font-medium text-text-secondary">Body</label>
            <textarea
              rows={4}
              value={editBody}
              onChange={(e) => setEditBody(e.target.value)}
              className="w-full rounded-lg border border-surface-border bg-surface-card px-3 py-2 text-sm text-text-primary placeholder:text-text-muted transition-colors focus:outline-none focus:ring-1 focus:ring-nebula-violet resize-none"
              required
            />
            {editError && <p className="text-xs text-red-400">{editError}</p>}
          </div>
          <TextInput
            label="Outcome"
            value={editOutcome}
            onChange={(e) => setEditOutcome(e.target.value)}
            placeholder="Optional outcome..."
          />
          <div className="flex justify-end gap-2 pt-1">
            <button type="button" onClick={() => setEditOpen(false)} className="rounded-lg border border-surface-border bg-surface-card px-4 py-2 text-sm font-medium text-text-secondary hover:bg-surface-card-hover">
              Cancel
            </button>
            <button type="submit" disabled={editMutation.isPending} className="rounded-lg bg-nebula-violet px-4 py-2 text-sm font-medium text-white hover:bg-nebula-violet/90 disabled:opacity-50">
              {editMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Redact Modal */}
      <Modal open={redactOpen} onClose={() => setRedactOpen(false)} title="Redact Communication" description="This action is permanent and cannot be undone. The body and outcome will be cleared.">
        <form onSubmit={handleRedact} className="space-y-3">
          <TextInput
            label="Reason"
            value={redactReason}
            onChange={(e) => { setRedactReason(e.target.value); setRedactError(''); }}
            placeholder="Compliance requirement, GDPR request..."
            required
            error={redactError}
          />
          <div className="flex justify-end gap-2 pt-1">
            <button type="button" onClick={() => setRedactOpen(false)} className="rounded-lg border border-surface-border bg-surface-card px-4 py-2 text-sm font-medium text-text-secondary hover:bg-surface-card-hover">
              Cancel
            </button>
            <button type="submit" disabled={redactMutation.isPending} className="rounded-lg bg-red-500 px-4 py-2 text-sm font-medium text-white hover:bg-red-600 disabled:opacity-50">
              {redactMutation.isPending ? 'Redacting...' : 'Redact'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Follow-Up Modal */}
      <Modal open={followUpOpen} onClose={() => setFollowUpOpen(false)} title="Create Follow-Up Task">
        <form onSubmit={handleFollowUp} className="space-y-3">
          <TextInput
            label="Title"
            value={followUpTitle}
            onChange={(e) => { setFollowUpTitle(e.target.value); setFollowUpError(''); }}
            placeholder="Follow up on..."
            required
            error={followUpError}
          />
          <div className="flex justify-end gap-2 pt-1">
            <button type="button" onClick={() => setFollowUpOpen(false)} className="rounded-lg border border-surface-border bg-surface-card px-4 py-2 text-sm font-medium text-text-secondary hover:bg-surface-card-hover">
              Cancel
            </button>
            <button type="submit" disabled={followUpMutation.isPending} className="rounded-lg bg-nebula-violet px-4 py-2 text-sm font-medium text-white hover:bg-nebula-violet/90 disabled:opacity-50">
              {followUpMutation.isPending ? 'Creating...' : 'Create Task'}
            </button>
          </div>
        </form>
      </Modal>
    </>
  );
}
