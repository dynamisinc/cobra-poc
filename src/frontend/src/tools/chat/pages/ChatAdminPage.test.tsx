/**
 * ChatAdminPage Component Tests
 *
 * Tests for the chat administration page.
 * Verifies channel listing, tab navigation, and channel management actions.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { MemoryRouter } from 'react-router-dom';
import { ChatAdminPage } from './ChatAdminPage';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock dependencies
vi.mock('../services/chatService', () => ({
  chatService: {
    getChannels: vi.fn(),
    getArchivedChannels: vi.fn(),
    createChannel: vi.fn(),
    deleteChannel: vi.fn(),
    restoreChannel: vi.fn(),
    permanentDeleteChannel: vi.fn(),
  },
}));

vi.mock('../../../shared/events', () => ({
  useEvents: vi.fn(),
}));

vi.mock('../../../shared/hooks/usePermissions', () => ({
  usePermissions: () => ({
    isManage: true,
    isReadonly: false,
    canInteractWithItems: true,
  }),
}));

vi.mock('../hooks/useChatHub', () => ({
  useChatHub: () => ({
    connectionState: 'connected',
    joinEventChat: vi.fn(),
    leaveEventChat: vi.fn(),
  }),
}));

vi.mock('react-toastify', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

// Mock dialog components
vi.mock('../components/CreateChannelDialog', () => ({
  CreateChannelDialog: ({ open, onClose }: { open: boolean; onClose: () => void }) =>
    open ? (
      <div data-testid="create-channel-dialog">
        <button onClick={onClose}>Close Create</button>
      </div>
    ) : null,
}));

vi.mock('../components/ArchiveChannelDialog', () => ({
  ArchiveChannelDialog: ({ open }: { open: boolean }) =>
    open ? <div data-testid="archive-channel-dialog">Archive Dialog</div> : null,
}));

vi.mock('../components/RestoreChannelDialog', () => ({
  RestoreChannelDialog: ({ open }: { open: boolean }) =>
    open ? <div data-testid="restore-channel-dialog">Restore Dialog</div> : null,
}));

vi.mock('../components/PermanentDeleteDialog', () => ({
  PermanentDeleteDialog: ({ open }: { open: boolean }) =>
    open ? <div data-testid="permanent-delete-dialog">Delete Dialog</div> : null,
}));

import { useEvents } from '../../../shared/events';

// Helper to render with providers
const renderWithProviders = (ui: React.ReactElement) => {
  return render(
    <ThemeProvider theme={cobraTheme}>
      <MemoryRouter>{ui}</MemoryRouter>
    </ThemeProvider>
  );
};

// Create mock channel
const createMockChannel = (
  overrides: Partial<ChatThreadDto> = {}
): ChatThreadDto => ({
  id: 'channel-1',
  eventId: 'event-1',
  name: 'General',
  channelType: ChannelType.Internal,
  channelTypeName: 'Internal',
  isDefaultEventThread: true,
  displayOrder: 0,
  messageCount: 10,
  createdAt: new Date().toISOString(),
  ...overrides,
});

describe('ChatAdminPage', () => {
  const mockEvent = {
    id: 'event-1',
    name: 'Test Event',
    eventTypeId: 'type-1',
    status: 'active',
    description: 'Test event description',
    startDate: new Date().toISOString(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('loading state', () => {
    it('shows loading spinner while event is loading', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: null,
        loading: true,
        events: [],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      renderWithProviders(<ChatAdminPage />);

      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });

    it('shows loading spinner while channels are loading', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getChannels).mockImplementation(() => new Promise(() => {}));
      vi.mocked(chatService.getArchivedChannels).mockImplementation(() => new Promise(() => {}));

      renderWithProviders(<ChatAdminPage />);

      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });
  });

  describe('no event selected', () => {
    it('shows warning when no event is selected', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: null,
        loading: false,
        events: [],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      renderWithProviders(<ChatAdminPage />);

      expect(screen.getByText(/Please select an event/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Go to Events/i })).toBeInTheDocument();
    });
  });

  describe('tab navigation', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getChannels).mockResolvedValue([createMockChannel()]);
      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([]);
    });

    it('displays Active and Archived tabs', async () => {
      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /Active Channels/i })).toBeInTheDocument();
        expect(screen.getByRole('tab', { name: /Archived Channels/i })).toBeInTheDocument();
      });
    });

    it('shows active channels by default', async () => {
      vi.mocked(chatService.getChannels).mockResolvedValue([
        createMockChannel({ name: 'Active Channel' }),
      ]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText('Active Channel')).toBeInTheDocument();
      });
    });

    it('switches to archived tab when clicked', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.getChannels).mockResolvedValue([]);
      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([
        createMockChannel({ id: 'arch-1', name: 'Archived Channel', isArchived: true }),
      ]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /Archived Channels/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('tab', { name: /Archived Channels/i }));

      await waitFor(() => {
        expect(screen.getByText('Archived Channel')).toBeInTheDocument();
      });
    });
  });

  describe('channel table', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([]);
    });

    it('displays channel name and type', async () => {
      vi.mocked(chatService.getChannels).mockResolvedValue([
        createMockChannel({ name: 'General Chat', channelTypeName: 'Internal' }),
      ]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText('General Chat')).toBeInTheDocument();
        expect(screen.getByText('Internal')).toBeInTheDocument();
      });
    });

    it('displays message count', async () => {
      vi.mocked(chatService.getChannels).mockResolvedValue([
        createMockChannel({ messageCount: 42 }),
      ]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText('42')).toBeInTheDocument();
      });
    });

    it('shows empty state when no active channels', async () => {
      vi.mocked(chatService.getChannels).mockResolvedValue([]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText(/No active channels/i)).toBeInTheDocument();
      });
    });

    it('shows error alert when loading fails', async () => {
      vi.mocked(chatService.getChannels).mockRejectedValue(new Error('Failed to load'));
      vi.mocked(chatService.getArchivedChannels).mockRejectedValue(new Error('Failed to load'));

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText(/Failed to load/i)).toBeInTheDocument();
      });
    });
  });

  describe('create channel button', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getChannels).mockResolvedValue([]);
      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([]);
    });

    it('shows create channel button', async () => {
      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /New Channel/i })).toBeInTheDocument();
      });
    });

    it('opens create dialog when clicked', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /New Channel/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /New Channel/i }));

      expect(screen.getByTestId('create-channel-dialog')).toBeInTheDocument();
    });
  });

  describe('channel actions', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([]);
    });

    it('shows action menu button for each channel', async () => {
      vi.mocked(chatService.getChannels).mockResolvedValue([
        createMockChannel({ name: 'Test Channel' }),
      ]);

      const { container } = renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByText('Test Channel')).toBeInTheDocument();
      });

      // Action menu button should be visible (ellipsis icon button in table)
      // The button doesn't have an aria-label, so we look for IconButton with the icon
      const menuButtons = container.querySelectorAll('button svg');
      expect(menuButtons.length).toBeGreaterThan(0);
    });
  });

  describe('archived channels tab', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(chatService.getChannels).mockResolvedValue([]);
    });

    it('shows empty state for archived channels', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /Archived Channels/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('tab', { name: /Archived Channels/i }));

      await waitFor(() => {
        expect(screen.getByText(/No archived channels/i)).toBeInTheDocument();
      });
    });

    it('shows archived channels in archived tab', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.getArchivedChannels).mockResolvedValue([
        createMockChannel({ id: 'arch-1', name: 'Old Channel', isArchived: true }),
        createMockChannel({ id: 'arch-2', name: 'Another Old', isArchived: true }),
      ]);

      renderWithProviders(<ChatAdminPage />);

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /Archived Channels/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('tab', { name: /Archived Channels/i }));

      await waitFor(() => {
        expect(screen.getByText('Old Channel')).toBeInTheDocument();
        expect(screen.getByText('Another Old')).toBeInTheDocument();
      });
    });
  });
});
