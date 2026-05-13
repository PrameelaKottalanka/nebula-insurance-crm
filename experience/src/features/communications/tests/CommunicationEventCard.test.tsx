import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/test-utils/render-app'
import { resetCommunicationMockState, seedCommunication } from '@/mocks/communications'
import { CommunicationEventCard } from '../components/CommunicationEventCard'
import type { CommunicationEventSummaryDto } from '../types'

const { mockGetUser } = vi.hoisted(() => ({ mockGetUser: vi.fn() }))

vi.mock('@/features/auth/oidcUserManager', () => ({
  oidcUserManager: {
    getUser: mockGetUser,
    events: {
      addUserLoaded: vi.fn(),
      addUserUnloaded: vi.fn(),
      removeUserLoaded: vi.fn(),
      removeUserUnloaded: vi.fn(),
    },
  },
}))

function asAdmin() {
  mockGetUser.mockResolvedValue({
    expired: false,
    access_token: 'test-token',
    profile: { sub: 'user-admin', name: 'Admin User', nebula_roles: ['Admin'] },
  })
}

function asAuthor() {
  mockGetUser.mockResolvedValue({
    expired: false,
    access_token: 'test-token',
    profile: { sub: 'user-author', name: 'Regular Author', nebula_roles: ['DistributionUser'] },
  })
}

function asOther() {
  mockGetUser.mockResolvedValue({
    expired: false,
    access_token: 'test-token',
    profile: { sub: 'user-other', name: 'Other User', nebula_roles: ['DistributionUser'] },
  })
}

const BASE_EVENT: CommunicationEventSummaryDto = {
  id: 'comm-1',
  eventType: 'Note',
  direction: null,
  durationMinutes: null,
  subject: 'Test subject',
  body: 'Test body content',
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
}

function renderCard(event: CommunicationEventSummaryDto = BASE_EVENT) {
  return renderWithProviders(
    <CommunicationEventCard event={event} entityType="Broker" entityId="broker-1" />,
  )
}

describe('CommunicationEventCard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    resetCommunicationMockState()
    asAdmin()
  })

  it('renders a Note event with subject, body and author', async () => {
    renderCard()

    expect(await screen.findByText('Test subject')).toBeInTheDocument()
    expect(screen.getByText('Test body content')).toBeInTheDocument()
    expect(screen.getByText('Test Author')).toBeInTheDocument()
    expect(screen.getByText('Note')).toBeInTheDocument()
  })

  it('renders a Call event with direction and duration', async () => {
    renderCard({
      ...BASE_EVENT,
      eventType: 'Call',
      direction: 'Outbound',
      durationMinutes: 30,
    })

    expect(await screen.findByText('Call')).toBeInTheDocument()
    expect(screen.getByText('Outbound')).toBeInTheDocument()
    expect(screen.getByText('30 min')).toBeInTheDocument()
  })

  it('renders a Meeting event icon', async () => {
    renderCard({ ...BASE_EVENT, eventType: 'Meeting', direction: 'Inbound', durationMinutes: 60 })

    expect(await screen.findByText('Meeting')).toBeInTheDocument()
    expect(screen.getByText('Inbound')).toBeInTheDocument()
    expect(screen.getByText('60 min')).toBeInTheDocument()
  })

  it('shows the outcome when present', async () => {
    renderCard({ ...BASE_EVENT, outcome: 'Deal agreed in principle' })

    expect(await screen.findByText('Deal agreed in principle')).toBeInTheDocument()
  })

  it('shows "edited" label when lastEditedAt is set', async () => {
    renderCard({ ...BASE_EVENT, lastEditedAt: new Date().toISOString() })

    expect(await screen.findByText('edited')).toBeInTheDocument()
  })

  it('shows Redacted badge and hides body/subject for redacted events', async () => {
    renderCard({
      ...BASE_EVENT,
      isRedacted: true,
      body: null,
      redactedByDisplayName: 'Admin User',
      redactedAt: new Date().toISOString(),
    })

    expect(await screen.findByText('Redacted')).toBeInTheDocument()
    expect(screen.queryByText('Test subject')).not.toBeInTheDocument()
    expect(screen.queryByText('Test body content')).not.toBeInTheDocument()
    expect(screen.getByText(/Redacted by Admin User/)).toBeInTheDocument()
  })

  it('shows follow-up task badge when followUpTaskId is set', async () => {
    renderCard({ ...BASE_EVENT, followUpTaskId: 'task-1', followUpTaskTitle: 'Call broker back' })

    expect(await screen.findByText('Follow-up: Call broker back')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: '+ Follow-up' })).not.toBeInTheDocument()
  })

  describe('Edit button visibility', () => {
    it('shows Edit for admin on any event', async () => {
      asAdmin()
      renderCard()
      expect(await screen.findByRole('button', { name: 'Edit' })).toBeInTheDocument()
    })

    it('shows Edit for author within the 24h window', async () => {
      asAuthor()
      renderCard({ ...BASE_EVENT, occurredAt: new Date().toISOString() })
      expect(await screen.findByRole('button', { name: 'Edit' })).toBeInTheDocument()
    })

    it('hides Edit for author outside the 24h window', async () => {
      asAuthor()
      const twoDaysAgo = new Date(Date.now() - 49 * 60 * 60 * 1000).toISOString()
      renderCard({ ...BASE_EVENT, occurredAt: twoDaysAgo })
      await screen.findByText('Test body content')
      expect(screen.queryByRole('button', { name: 'Edit' })).not.toBeInTheDocument()
    })

    it('hides Edit for a non-author non-admin', async () => {
      asOther()
      renderCard()
      await screen.findByText('Test body content')
      expect(screen.queryByRole('button', { name: 'Edit' })).not.toBeInTheDocument()
    })

    it('hides Edit for redacted events even for admin', async () => {
      asAdmin()
      renderCard({ ...BASE_EVENT, isRedacted: true, body: null, redactedByDisplayName: 'Admin', redactedAt: new Date().toISOString() })
      await screen.findByText('Redacted')
      expect(screen.queryByRole('button', { name: 'Edit' })).not.toBeInTheDocument()
    })
  })

  describe('Redact button visibility', () => {
    it('shows Redact button for admin on non-redacted events', async () => {
      asAdmin()
      renderCard()
      expect(await screen.findByRole('button', { name: 'Redact' })).toBeInTheDocument()
    })

    it('hides Redact for non-admin', async () => {
      asAuthor()
      renderCard()
      await screen.findByText('Test body content')
      expect(screen.queryByRole('button', { name: 'Redact' })).not.toBeInTheDocument()
    })

    it('hides Redact for already-redacted events', async () => {
      asAdmin()
      renderCard({ ...BASE_EVENT, isRedacted: true, body: null, redactedByDisplayName: 'Admin', redactedAt: new Date().toISOString() })
      await screen.findByText('Redacted')
      expect(screen.queryByRole('button', { name: 'Redact' })).not.toBeInTheDocument()
    })
  })

  describe('Edit modal', () => {
    it('opens with pre-populated fields and validates empty body', async () => {
      asAdmin()
      seedCommunication({ id: 'comm-1', subject: 'Test subject', body: 'Test body content', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: 'Edit' }))
      const dialog = await screen.findByRole('dialog', { name: 'Edit Communication' })
      expect(dialog).toBeInTheDocument()

      const textarea = dialog.querySelector('textarea')!
      await user.clear(textarea)
      await user.click(screen.getByRole('button', { name: 'Save' }))

      expect(await screen.findByText('Body is required.')).toBeInTheDocument()
    })

    it('saves edits and closes the modal', async () => {
      asAdmin()
      seedCommunication({ id: 'comm-1', subject: 'Test subject', body: 'Test body content', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: 'Edit' }))
      const dialog = await screen.findByRole('dialog', { name: 'Edit Communication' })

      const textarea = dialog.querySelector('textarea')!
      await user.clear(textarea)
      await user.type(textarea, 'Updated body text')
      await user.click(screen.getByRole('button', { name: 'Save' }))

      await waitFor(() => {
        expect(screen.queryByRole('dialog', { name: 'Edit Communication' })).not.toBeInTheDocument()
      })
    })
  })

  describe('Redact modal', () => {
    it('validates that reason is required', async () => {
      asAdmin()
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: 'Redact' }))
      await screen.findByRole('dialog', { name: 'Redact Communication' })
      await user.click(screen.getByRole('button', { name: 'Redact' }))

      expect(await screen.findByText('Reason is required.')).toBeInTheDocument()
    })

    it('submits a redaction and closes the modal', async () => {
      asAdmin()
      seedCommunication({ id: 'comm-1', body: 'Test body content', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: 'Redact' }))
      const dialog = await screen.findByRole('dialog', { name: 'Redact Communication' })
      await user.type(dialog.querySelector('input')!, 'GDPR request')
      await user.click(screen.getByRole('button', { name: 'Redact' }))

      await waitFor(() => {
        expect(screen.queryByRole('dialog', { name: 'Redact Communication' })).not.toBeInTheDocument()
      })
    })
  })

  describe('Follow-up modal', () => {
    it('validates that title is required', async () => {
      asAdmin()
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: '+ Follow-up' }))
      await screen.findByRole('dialog', { name: 'Create Follow-Up Task' })
      await user.click(screen.getByRole('button', { name: 'Create Task' }))

      expect(await screen.findByText('Title is required.')).toBeInTheDocument()
    })

    it('creates a follow-up task and shows the badge', async () => {
      asAdmin()
      seedCommunication({ id: 'comm-1', body: 'Test body content', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })
      const user = userEvent.setup()

      renderCard()

      await user.click(await screen.findByRole('button', { name: '+ Follow-up' }))
      const dialog = await screen.findByRole('dialog', { name: 'Create Follow-Up Task' })
      await user.type(dialog.querySelector('input')!, 'Call broker next week')
      await user.click(screen.getByRole('button', { name: 'Create Task' }))

      await waitFor(() => {
        expect(screen.queryByRole('dialog', { name: 'Create Follow-Up Task' })).not.toBeInTheDocument()
      })
    })
  })
})
