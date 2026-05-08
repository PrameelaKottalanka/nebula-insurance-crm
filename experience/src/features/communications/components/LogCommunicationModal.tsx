import { useState } from 'react';
import { Modal } from '@/components/ui/Modal';
import { TextInput } from '@/components/ui/TextInput';
import { Select } from '@/components/ui/Select';
import { useLogCommunication } from '../hooks/useCommunicationMutations';
import type { CommunicationEventType, CommunicationDirection } from '../types';

interface Props {
  open: boolean;
  onClose: () => void;
  entityType: string;
  entityId: string;
}

type Tab = CommunicationEventType;

const DIRECTION_OPTIONS = [
  { value: 'Inbound', label: 'Inbound' },
  { value: 'Outbound', label: 'Outbound' },
];

interface FormState {
  subject: string;
  body: string;
  outcome: string;
  direction: CommunicationDirection | '';
  durationMinutes: string;
  occurredAt: string;
}

const EMPTY_FORM: FormState = {
  subject: '',
  body: '',
  outcome: '',
  direction: '',
  durationMinutes: '',
  occurredAt: '',
};

export function LogCommunicationModal({ open, onClose, entityType, entityId }: Props) {
  const [activeTab, setActiveTab] = useState<Tab>('Note');
  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [errors, setErrors] = useState<Partial<Record<keyof FormState, string>>>({});

  const { mutate: logCommunication, isPending } = useLogCommunication(entityType, entityId);

  function patch(field: Partial<FormState>) {
    setForm((prev) => ({ ...prev, ...field }));
  }

  function validate(): boolean {
    const next: typeof errors = {};
    if (!form.body.trim()) next.body = 'Body is required.';
    if (activeTab === 'Call') {
      if (!form.direction) next.direction = 'Direction is required for calls.';
      if (!form.durationMinutes) next.durationMinutes = 'Duration is required for calls.';
      else if (Number(form.durationMinutes) < 1) next.durationMinutes = 'Duration must be at least 1 minute.';
    }
    if (activeTab === 'Meeting') {
      if (!form.direction) next.direction = 'Direction is required for meetings.';
      if (!form.durationMinutes) next.durationMinutes = 'Duration is required for meetings.';
      else if (Number(form.durationMinutes) < 1) next.durationMinutes = 'Duration must be at least 1 minute.';
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    logCommunication(
      {
        eventType: activeTab,
        direction: form.direction || undefined,
        durationMinutes: form.durationMinutes ? Number(form.durationMinutes) : undefined,
        subject: form.subject.trim() || undefined,
        body: form.body.trim(),
        outcome: form.outcome.trim() || undefined,
        occurredAt: form.occurredAt || undefined,
        primaryEntityType: entityType,
        primaryEntityId: entityId,
      },
      {
        onSuccess: () => {
          setForm(EMPTY_FORM);
          setErrors({});
          setActiveTab('Note');
          onClose();
        },
      },
    );
  }

  function handleClose() {
    setForm(EMPTY_FORM);
    setErrors({});
    setActiveTab('Note');
    onClose();
  }

  const tabs: Tab[] = ['Note', 'Call', 'Meeting'];

  return (
    <Modal open={open} onClose={handleClose} title="Log Communication">
      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        {/* Type switcher */}
        <div className="flex rounded-lg border border-surface-border bg-surface-main p-0.5 gap-0.5">
          {tabs.map((tab) => (
            <button
              key={tab}
              type="button"
              onClick={() => { setActiveTab(tab); setErrors({}); }}
              className={`flex-1 rounded-md py-1.5 text-xs font-medium transition-colors ${
                activeTab === tab
                  ? 'bg-nebula-violet text-white'
                  : 'text-text-muted hover:text-text-secondary'
              }`}
            >
              {tab === 'Note' ? '📝 Note' : tab === 'Call' ? '📞 Call' : '🤝 Meeting'}
            </button>
          ))}
        </div>

        <TextInput
          label="Subject"
          value={form.subject}
          onChange={(e) => patch({ subject: e.target.value })}
          placeholder="Optional subject..."
        />

        <div className="space-y-1.5">
          <label className="block text-xs font-medium text-text-secondary">
            Body <span className="text-red-400">*</span>
          </label>
          <textarea
            rows={4}
            value={form.body}
            onChange={(e) => patch({ body: e.target.value })}
            placeholder="What happened..."
            className="w-full rounded-lg border border-surface-border bg-surface-card px-3 py-2 text-sm text-text-primary placeholder:text-text-muted transition-colors focus:outline-none focus:ring-1 focus:ring-nebula-violet resize-none"
          />
          {errors.body && <p className="text-xs text-red-400">{errors.body}</p>}
        </div>

        {(activeTab === 'Call' || activeTab === 'Meeting') && (
          <div className="grid grid-cols-2 gap-3">
            <Select
              label="Direction"
              value={form.direction}
              onChange={(e) => patch({ direction: e.target.value as CommunicationDirection | '' })}
              options={DIRECTION_OPTIONS}
              placeholder="Select..."
              error={errors.direction}
            />
            <TextInput
              label="Duration (minutes)"
              type="number"
              value={form.durationMinutes}
              onChange={(e) => patch({ durationMinutes: e.target.value })}
              placeholder="30"
              error={errors.durationMinutes}
            />
          </div>
        )}

        <TextInput
          label="Outcome"
          value={form.outcome}
          onChange={(e) => patch({ outcome: e.target.value })}
          placeholder="Optional outcome or next steps..."
        />

        <TextInput
          label="Occurred At"
          type="datetime-local"
          value={form.occurredAt}
          onChange={(e) => patch({ occurredAt: e.target.value })}
        />

        <div className="flex justify-end gap-2 pt-1">
          <button
            type="button"
            onClick={handleClose}
            className="rounded-lg border border-surface-border bg-surface-card px-4 py-2 text-sm font-medium text-text-secondary transition-colors hover:bg-surface-card-hover hover:text-text-primary"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isPending}
            className="rounded-lg bg-nebula-violet px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-nebula-violet/90 disabled:opacity-50"
          >
            {isPending ? 'Logging...' : `Log ${activeTab}`}
          </button>
        </div>
      </form>
    </Modal>
  );
}
