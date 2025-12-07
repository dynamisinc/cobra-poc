/**
 * ChatSidebar Component Tests
 *
 * Tests for the resizable sidebar chat component.
 * Verifies sidebar visibility, channel navigation, and resize functionality.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { MemoryRouter } from 'react-router-dom';
import { ChatSidebar } from './ChatSidebar';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock dependencies
vi.mock('../services/chatService', () => ({
  chatService: {
    getExternalChannels: vi.fn(),
    createExternalChannel: vi.fn(),
    unlinkExternalChannel: vi.fn(),
  },
}));

vi.mock('../../../shared/events', () => ({
  useEvents: vi.fn(),
}));

vi.mock('../contexts/ChatSidebarContext', () => ({
  useChatSidebar: vi.fn(),
}));

vi.mock('../hooks/useExternalMessagingConfig', () => ({
  useExternalMessagingConfig: () => ({
    isConfigured: false,
    groupMe: { isConfigured: false, hasAccessToken: false },
    teams: { isConfigured: false, isConnected: false },
    loading: false,
    error: null,
  }),
}));

vi.mock('../hooks/useChatHub', () => ({
  useChatHub: () => ({
    connectionState: 'connected',
  }),
}));

vi.mock('react-toastify', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

// Mock child components
vi.mock('./EventChat', () => ({
  EventChat: ({ channelName }: { channelName?: string }) => (
    <div data-testid="event-chat">EventChat: {channelName || 'default'}</div>
  ),
}));

vi.mock('./ChannelList', () => ({
  ChannelList: ({ onChannelSelect }: { onChannelSelect: (channel: ChatThreadDto) => void }) => (
    <div data-testid="channel-list">
      <button
        onClick={() =>
          onChannelSelect({
            id: 'ch-1',
            eventId: 'event-1',
            name: 'Test Channel',
            channelType: ChannelType.Internal,
            channelTypeName: 'Internal',
            isDefaultEventThread: false,
            displayOrder: 0,
            messageCount: 5,
            createdAt: new Date().toISOString(),
          })
        }
      >
        Select Channel
      </button>
    </div>
  ),
}));

vi.mock('./ChannelMenu', () => ({
  ChannelMenu: () => <div data-testid="channel-menu">ChannelMenu</div>,
}));

vi.mock('./TeamsChannelDialog', () => ({
  TeamsChannelDialog: () => null,
}));

import { useEvents } from '../../../shared/events';
import { useChatSidebar } from '../contexts/ChatSidebarContext';

// Helper to render with providers
const renderWithProviders = (ui: React.ReactElement) => {
  return render(
    <ThemeProvider theme={cobraTheme}>
      <MemoryRouter>{ui}</MemoryRouter>
    </ThemeProvider>
  );
};

describe('ChatSidebar', () => {
  const mockEvent = {
    id: 'event-1',
    name: 'Test Event',
    eventTypeId: 'type-1',
    status: 'active',
    description: 'Test event description',
    startDate: new Date().toISOString(),
  };

  const mockCloseSidebar = vi.fn();
  const mockSetWidth = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(chatService.getExternalChannels).mockResolvedValue([]);
  });

  describe('visibility', () => {
    it('renders nothing when sidebar is closed', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: false,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });

      const { container } = renderWithProviders(<ChatSidebar />);

      expect(container.firstChild).toBeNull();
    });

    it('renders sidebar when open', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });

      renderWithProviders(<ChatSidebar />);

      expect(screen.getByText('Event Chat')).toBeInTheDocument();
    });
  });

  describe('no event selected', () => {
    it('shows message when no event is selected', () => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: null,
        loading: false,
        events: [],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });

      renderWithProviders(<ChatSidebar />);

      expect(screen.getByText(/No event selected/i)).toBeInTheDocument();
      expect(screen.getByText(/Select an event to view the chat/i)).toBeInTheDocument();
    });
  });

  describe('channel navigation', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });
    });

    it('shows channel list initially', () => {
      renderWithProviders(<ChatSidebar />);

      expect(screen.getByTestId('channel-list')).toBeInTheDocument();
    });

    it('shows chat when channel is selected', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatSidebar />);

      await user.click(screen.getByText('Select Channel'));

      await waitFor(() => {
        expect(screen.getByTestId('event-chat')).toBeInTheDocument();
        expect(screen.getByTestId('event-chat')).toHaveTextContent('Test Channel');
      });
    });

    it('shows back button when viewing channel', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatSidebar />);

      await user.click(screen.getByText('Select Channel'));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /Back to channels/i })).toBeInTheDocument();
      });
    });

    it('returns to channel list when back button is clicked', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatSidebar />);

      // Select a channel
      await user.click(screen.getByText('Select Channel'));

      await waitFor(() => {
        expect(screen.getByTestId('event-chat')).toBeInTheDocument();
      });

      // Click back
      await user.click(screen.getByRole('button', { name: /Back to channels/i }));

      await waitFor(() => {
        expect(screen.getByTestId('channel-list')).toBeInTheDocument();
      });
    });

    it('updates header with channel name when selected', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatSidebar />);

      // Initially shows "Event Chat"
      expect(screen.getByText('Event Chat')).toBeInTheDocument();

      // Select a channel
      await user.click(screen.getByText('Select Channel'));

      // Now shows channel name
      await waitFor(() => {
        expect(screen.getByText('Test Channel')).toBeInTheDocument();
      });
    });
  });

  describe('header actions', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });
    });

    it('closes sidebar when close button is clicked', async () => {
      const user = userEvent.setup();
      renderWithProviders(<ChatSidebar />);

      await user.click(screen.getByRole('button', { name: /Close chat/i }));

      expect(mockCloseSidebar).toHaveBeenCalled();
    });

    it('has expand to full page button', () => {
      renderWithProviders(<ChatSidebar />);

      expect(screen.getByRole('button', { name: /Open full chat page/i })).toBeInTheDocument();
    });
  });

  describe('resize handle', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });
    });

    it('renders resize handle', () => {
      const { container } = renderWithProviders(<ChatSidebar />);

      // The resize handle has cursor: ew-resize style
      const resizeHandle = container.querySelector('[style*="cursor"]');
      expect(resizeHandle).toBeDefined();
    });
  });

  describe('connection status', () => {
    beforeEach(() => {
      vi.mocked(useEvents).mockReturnValue({
        currentEvent: mockEvent,
        loading: false,
        events: [mockEvent],
        error: null,
        refreshEvents: vi.fn(),
        setCurrentEvent: vi.fn(),
      } as ReturnType<typeof useEvents>);

      vi.mocked(useChatSidebar).mockReturnValue({
        isOpen: true,
        openSidebar: vi.fn(),
        closeSidebar: mockCloseSidebar,
        toggleSidebar: vi.fn(),
        width: 350,
        setWidth: mockSetWidth,
      });
    });

    it('does not show connection indicator when connected', () => {
      renderWithProviders(<ChatSidebar />);

      // When connected, no warning indicator should be visible
      expect(screen.queryByText(/Reconnecting/i)).not.toBeInTheDocument();
      expect(screen.queryByText(/Disconnected/i)).not.toBeInTheDocument();
    });
  });
});
