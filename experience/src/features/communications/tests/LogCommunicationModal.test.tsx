import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/test-utils/render-app'
import { resetCommunicationMockState } from '@/mocks/communications'
import { LogCommunicationModal } from '../components/LogCommunicationModal'

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

function renderModal(onClose = vi.fn()) {
  return renderWithProviders(
    <LogCommunicationModal
      open
      onClose={onClose}
      entityType="Broker"
      entityId="broker-1"
    />,
  )
}

async function getDialog() {
  return screen.findByRole('dialog', { name: 'Log Communication' })
}

describe('LogCommunicationModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    resetCommunicationMockState()
    mockGetUser.mockResolvedValue({
      expired: false,
      access_token: 'test-token',
      profile: { sub: 'user-1', name: 'User One', nebula_roles: ['DistributionUser'] },
    })
  })

  it('renders with Note tab active by default', async () => {
    renderModal()

    const dialog = await getDialog()
    expect(dialog).toBeInTheDocument()
    expect(within(dialog).getByRole('button', { name: '📝 Note' })).toBeInTheDocument()
    expect(within(dialog).getByRole('button', { name: 'Log Note' })).toBeInTheDocument()
    expect(screen.queryByLabelText('Direction')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('Duration (minutes)')).not.toBeInTheDocument()
  })

  it('switches to Call tab and shows direction and duration fields', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))

    expect(within(dialog).getByRole('button', { name: 'Log Call' })).toBeInTheDocument()
    expect(within(dialog).getByLabelText('Direction')).toBeInTheDocument()
    expect(within(dialog).getByLabelText('Duration (minutes)')).toBeInTheDocument()
  })

  it('switches to Meeting tab and shows direction and duration fields', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '🤝 Meeting' }))

    expect(within(dialog).getByRole('button', { name: 'Log Meeting' })).toBeInTheDocument()
    expect(within(dialog).getByLabelText('Direction')).toBeInTheDocument()
    expect(within(dialog).getByLabelText('Duration (minutes)')).toBeInTheDocument()
  })

  it('hides direction and duration when switching back to Note', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))
    await user.click(within(dialog).getByRole('button', { name: '📝 Note' }))

    expect(within(dialog).getByRole('button', { name: 'Log Note' })).toBeInTheDocument()
    expect(screen.queryByLabelText('Direction')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('Duration (minutes)')).not.toBeInTheDocument()
  })

  it('validates that body is required for a Note', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: 'Log Note' }))

    expect(await screen.findByText('Body is required.')).toBeInTheDocument()
  })

  it('validates that direction and duration are required for a Call', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))
    await user.type(screen.getByPlaceholderText('What happened...'), 'Some call notes')
    await user.click(within(dialog).getByRole('button', { name: 'Log Call' }))

    expect(await screen.findByText('Direction is required for calls.')).toBeInTheDocument()
    expect(screen.getByText('Duration is required for calls.')).toBeInTheDocument()
  })

  it('validates that duration must be at least 1 minute', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))
    await user.type(screen.getByPlaceholderText('What happened...'), 'Call notes')
    await user.selectOptions(within(dialog).getByLabelText('Direction'), 'Outbound')
    await user.type(within(dialog).getByLabelText('Duration (minutes)'), '0')
    await user.click(within(dialog).getByRole('button', { name: 'Log Call' }))

    expect(await screen.findByText('Duration must be at least 1 minute.')).toBeInTheDocument()
  })

  it('clears validation errors when switching tabs', async () => {
    const user = userEvent.setup()
    renderModal()

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: 'Log Note' }))
    expect(await screen.findByText('Body is required.')).toBeInTheDocument()

    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))
    expect(screen.queryByText('Body is required.')).not.toBeInTheDocument()
  })

  it('successfully logs a Note and closes the modal', async () => {
    const onClose = vi.fn()
    const user = userEvent.setup()
    renderModal(onClose)

    const dialog = await getDialog()
    await user.type(screen.getByPlaceholderText('What happened...'), 'Spoke to the broker about renewal.')
    await user.click(within(dialog).getByRole('button', { name: 'Log Note' }))

    await waitFor(() => expect(onClose).toHaveBeenCalled())
  })

  it('successfully logs a Call with direction and duration', async () => {
    const onClose = vi.fn()
    const user = userEvent.setup()
    renderModal(onClose)

    const dialog = await getDialog()
    await user.click(within(dialog).getByRole('button', { name: '📞 Call' }))
    await user.type(screen.getByPlaceholderText('What happened...'), 'Discussed coverage limits.')
    await user.selectOptions(within(dialog).getByLabelText('Direction'), 'Outbound')
    await user.type(within(dialog).getByLabelText('Duration (minutes)'), '20')
    await user.click(within(dialog).getByRole('button', { name: 'Log Call' }))

    await waitFor(() => expect(onClose).toHaveBeenCalled())
  })

  it('resets the form and closes when Cancel is clicked', async () => {
    const onClose = vi.fn()
    const user = userEvent.setup()
    renderModal(onClose)

    const dialog = await getDialog()
    await user.type(screen.getByPlaceholderText('What happened...'), 'Some draft text')
    await user.click(within(dialog).getByRole('button', { name: 'Cancel' }))

    expect(onClose).toHaveBeenCalled()
  })
})
