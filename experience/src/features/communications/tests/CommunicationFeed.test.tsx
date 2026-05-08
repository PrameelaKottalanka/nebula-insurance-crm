import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/test-utils/render-app'
import { server } from '@/mocks/server'
import { API_ORIGIN } from '@/mocks/data'
import { resetCommunicationMockState, seedCommunication } from '@/mocks/communications'
import { CommunicationFeed } from '../components/CommunicationFeed'

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

function renderFeed() {
  return renderWithProviders(
    <CommunicationFeed entityType="Broker" entityId="broker-1" />,
  )
}

describe('CommunicationFeed', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    resetCommunicationMockState()
    asAdmin()
  })

  it('shows the log communication button immediately', () => {
    renderFeed()
    expect(screen.getByRole('button', { name: '+ Log Communication' })).toBeInTheDocument()
  })

  it('shows empty state with CTA when there are no communications', async () => {
    renderFeed()
    expect(await screen.findByText('No communications yet')).toBeInTheDocument()
    expect(screen.getByText('Log a note, call, or meeting to get started.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Log Communication' })).toBeInTheDocument()
  })

  it('shows the total count and list of communications', async () => {
    seedCommunication({ id: 'comm-1', subject: 'First note', body: 'Body one', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })
    seedCommunication({ id: 'comm-2', subject: 'Second note', body: 'Body two', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })

    renderFeed()

    expect(await screen.findByText('2 communications')).toBeInTheDocument()
    expect(screen.getByText('First note')).toBeInTheDocument()
    expect(screen.getByText('Second note')).toBeInTheDocument()
  })

  it('shows singular count label for exactly one communication', async () => {
    seedCommunication({ id: 'comm-1', body: 'Solo note', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })

    renderFeed()

    expect(await screen.findByText('1 communication')).toBeInTheDocument()
  })

  it('shows an error state when the API fails', async () => {
    server.use(
      http.get(`${API_ORIGIN}/communications`, () =>
        HttpResponse.json({ title: 'Server error' }, { status: 500 }),
      ),
    )

    renderFeed()

    expect(await screen.findByText('Failed to load communications. Please try again.')).toBeInTheDocument()
  })

  it('opens the log modal when the toolbar button is clicked', async () => {
    const user = userEvent.setup()
    renderFeed()

    await screen.findByText('No communications yet')
    await user.click(screen.getByRole('button', { name: '+ Log Communication' }))

    expect(await screen.findByRole('dialog', { name: 'Log Communication' })).toBeInTheDocument()
  })

  it('opens the log modal from the empty state CTA', async () => {
    const user = userEvent.setup()
    renderFeed()

    await user.click(await screen.findByRole('button', { name: 'Log Communication' }))

    expect(await screen.findByRole('dialog', { name: 'Log Communication' })).toBeInTheDocument()
  })

  it('does not show pagination when all items fit on one page', async () => {
    seedCommunication({ id: 'comm-1', body: 'Solo', primaryEntityType: 'Broker', primaryEntityId: 'broker-1' })

    renderFeed()

    await screen.findByText('1 communication')
    expect(screen.queryByRole('button', { name: 'Previous' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Next' })).not.toBeInTheDocument()
  })

  it('shows pagination and navigates when totalPages > 1', async () => {
    server.use(
      http.get(`${API_ORIGIN}/communications`, ({ request }) => {
        const page = Number(new URL(request.url).searchParams.get('page') ?? '1')
        return HttpResponse.json({
          data: [{ id: `comm-page-${page}`, eventType: 'Note', body: `Page ${page} body`, direction: null, durationMinutes: null, subject: `Page ${page} note`, outcome: null, occurredAt: new Date().toISOString(), primaryEntityType: 'Broker', primaryEntityId: 'broker-1', authoredByUserId: 'u1', authoredByDisplayName: 'Author', lastEditedAt: null, isRedacted: false, redactedByDisplayName: null, redactedAt: null, redactionReason: null, followUpTaskId: null, followUpTaskTitle: null, followUpTaskStatus: null }],
          page,
          pageSize: 25,
          totalCount: 50,
          totalPages: 2,
        })
      }),
    )

    const user = userEvent.setup()
    renderFeed()

    expect(await screen.findByText('Page 1 note')).toBeInTheDocument()
    expect(screen.getByText('Page 1 of 2')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Previous' })).toBeDisabled()

    await user.click(screen.getByRole('button', { name: 'Next' }))

    expect(await screen.findByText('Page 2 note')).toBeInTheDocument()
    expect(screen.getByText('Page 2 of 2')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Next' })).toBeDisabled()
  })

  it('logs a new note and refreshes the feed', async () => {
    const user = userEvent.setup()
    renderFeed()

    await screen.findByText('No communications yet')
    await user.click(screen.getByRole('button', { name: '+ Log Communication' }))

    const dialog = await screen.findByRole('dialog', { name: 'Log Communication' })
    await user.type(screen.getByPlaceholderText('What happened...'), 'My first note')
    await user.click(within(dialog).getByRole('button', { name: 'Log Note' }))

    await waitFor(() => {
      expect(screen.queryByRole('dialog', { name: 'Log Communication' })).not.toBeInTheDocument()
    })

    expect(await screen.findByText('My first note')).toBeInTheDocument()
  })
})
