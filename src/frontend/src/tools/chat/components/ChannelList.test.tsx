/**
 * ChannelList Component Tests
 *
 * Tests for the channel list component that displays event channels
 * in an accordion-style grouped list.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { ChannelList } from './ChannelList';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock the chatService
vi.mock('../services/chatService', () => ({
  chatService: {
    getUserVisibleChannels: vi.fn(),
    createChannel: vi.fn(),
    deleteChannel: vi.fn(),
  },
}));

// Mock the external messaging config hook
vi.mock('../hooks/useExternalMessagingConfig', () => ({
  useExternalMessagingConfig: () => ({
    isConfigured: true,
    groupMe: { isConfigured: true, hasAccessToken: true },
    teams: { isConfigured: true, isConnected: true, availableConversations: 2 },
    loading: false,
    error: null,
  }),
}));

// Helper to render with theme provider
const renderWithTheme = (ui: React.ReactElement) => {
  return render(<ThemeProvider theme={cobraTheme}>{ui}</ThemeProvider>);
};

// Create mock channel
const createMockChannel = (
  overrides: Partial<ChatThreadDto> = {}
): ChatThreadDto => ({
  id: 'channel-1',
  eventId: 'event-1',
  name: 'Test Channel',
  channelType: ChannelType.Internal,
  channelTypeName: 'Internal',
  isDefaultEventThread: false,
  displayOrder: 0,
  messageCount: 0,
  createdAt: new Date().toISOString(),
  ...overrides,
});

describe('ChannelList', () => {
  const mockOnChannelSelect = vi.fn();

  const defaultProps = {
    eventId: 'event-1',
    onChannelSelect: mockOnChannelSelect,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('loading state', () => {
    it('shows skeleton loaders while loading', () => {
      vi.mocked(chatService.getUserVisibleChannels).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<ChannelList {...defaultProps} />);

      // Should show skeleton elements
      const skeletons = document.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe('error state', () => {
    it('shows error message when fetch fails', async () => {
      vi.mocked(chatService.getUserVisibleChannels).mockRejectedValue(
        new Error('Network error')
      );

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Network error')).toBeInTheDocument();
      });
    });
  });

  describe('channel display', () => {
    it('displays internal channels in Channels section', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Event Chat',
          channelType: ChannelType.Internal,
          isDefaultEventThread: true,
        }),
        createMockChannel({
          id: 'ch-2',
          name: 'Announcements',
          channelType: ChannelType.Announcements,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Event Chat')).toBeInTheDocument();
        expect(screen.getByText('Announcements')).toBeInTheDocument();
      });
    });

    it('displays position channels in My Sections', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Operations',
          channelType: ChannelType.Position,
          positionName: 'Operations Section Chief',
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('My Sections')).toBeInTheDocument();
        expect(screen.getByText('Operations')).toBeInTheDocument();
      });
    });

    it('displays external channels in External section', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Teams Channel',
          channelType: ChannelType.External,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('External')).toBeInTheDocument();
        expect(screen.getByText('Teams Channel')).toBeInTheDocument();
      });
    });

    it('displays custom channels in Groups section', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Planning Team',
          channelType: ChannelType.Custom,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Groups')).toBeInTheDocument();
        expect(screen.getByText('Planning Team')).toBeInTheDocument();
      });
    });

    it('shows message count badge', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Active Channel',
          messageCount: 42,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('42')).toBeInTheDocument();
      });
    });

    it('caps message count badge at 99', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Busy Channel',
          messageCount: 150,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('99+')).toBeInTheDocument();
      });
    });
  });

  describe('channel selection', () => {
    it('calls onChannelSelect when channel is clicked', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel({ id: 'ch-1', name: 'Selectable Channel' });
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([channel]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Selectable Channel')).toBeInTheDocument();
      });

      await user.click(screen.getByText('Selectable Channel'));

      expect(mockOnChannelSelect).toHaveBeenCalledWith(channel);
    });

    it('highlights selected channel', async () => {
      const channel = createMockChannel({ id: 'ch-1', name: 'Selected Channel' });
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([channel]);

      renderWithTheme(
        <ChannelList {...defaultProps} selectedChannelId="ch-1" />
      );

      await waitFor(() => {
        const channelButton = screen.getByRole('button', { name: /Selected Channel/i });
        expect(channelButton).toHaveClass('Mui-selected');
      });
    });
  });

  describe('section toggling', () => {
    it('collapses and expands sections', async () => {
      const user = userEvent.setup();
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Internal Channel',
          channelType: ChannelType.Internal,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Internal Channel')).toBeVisible();
      });

      // Click section header to collapse
      await user.click(screen.getByText('Channels'));

      // Channel should be hidden
      await waitFor(() => {
        expect(screen.queryByText('Internal Channel')).not.toBeVisible();
      });

      // Click again to expand
      await user.click(screen.getByText('Channels'));

      await waitFor(() => {
        expect(screen.getByText('Internal Channel')).toBeVisible();
      });
    });
  });

  describe('channel archiving', () => {
    it('shows archive menu for custom channels', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel({
        id: 'ch-1',
        name: 'Custom Group',
        channelType: ChannelType.Custom,
        isDefaultEventThread: false,
      });
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([channel]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Custom Group')).toBeInTheDocument();
      });

      // Expand Groups section
      await user.click(screen.getByText('Groups'));

      // Hover over the channel to show menu button
      const channelItem = screen.getByRole('button', { name: /Custom Group/i });
      await user.hover(channelItem);

      // Find and click the menu button
      const menuButton = within(channelItem).getByRole('button');
      await user.click(menuButton);

      // Archive option should be visible
      expect(screen.getByText('Archive')).toBeInTheDocument();
    });

    it('does not show archive menu for default channels', async () => {
      const channel = createMockChannel({
        id: 'ch-1',
        name: 'Event Chat',
        channelType: ChannelType.Internal,
        isDefaultEventThread: true,
      });
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([channel]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Event Chat')).toBeInTheDocument();
      });

      // The channel item should not have a menu button
      const channelItem = screen.getByRole('button', { name: /Event Chat/i });
      // Only the channel button itself, no nested menu button
      const buttons = within(channelItem).queryAllByRole('button');
      expect(buttons).toHaveLength(0);
    });

    it('does not show archive menu for external channels', async () => {
      const channel = createMockChannel({
        id: 'ch-1',
        name: 'Teams Link',
        channelType: ChannelType.External,
      });
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([channel]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Teams Link')).toBeInTheDocument();
      });

      const channelItem = screen.getByRole('button', { name: /Teams Link/i });
      const buttons = within(channelItem).queryAllByRole('button');
      expect(buttons).toHaveLength(0);
    });
  });

  describe('empty states', () => {
    it('shows empty message when no channels in section', async () => {
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('No channels available')).toBeInTheDocument();
      });
    });

    it('shows empty message for External section when no external channels', async () => {
      const channels = [
        createMockChannel({
          id: 'ch-1',
          name: 'Internal',
          channelType: ChannelType.Internal,
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('No external channels connected')).toBeInTheDocument();
      });
    });
  });

  describe('refresh behavior', () => {
    it('reloads channels when refreshKey changes', async () => {
      const channels = [createMockChannel({ name: 'Initial Channel' })];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      const { rerender } = renderWithTheme(
        <ChannelList {...defaultProps} refreshKey={0} />
      );

      await waitFor(() => {
        expect(chatService.getUserVisibleChannels).toHaveBeenCalledTimes(1);
      });

      // Update refresh key
      rerender(
        <ThemeProvider theme={cobraTheme}>
          <ChannelList {...defaultProps} refreshKey={1} />
        </ThemeProvider>
      );

      await waitFor(() => {
        expect(chatService.getUserVisibleChannels).toHaveBeenCalledTimes(2);
      });
    });
  });

  describe('compact mode', () => {
    it('renders in compact mode', async () => {
      const channels = [createMockChannel({ name: 'Compact Channel' })];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} compact={true} />);

      await waitFor(() => {
        expect(screen.getByText('Compact Channel')).toBeInTheDocument();
      });
    });

    it('hides description in compact mode', async () => {
      const channels = [
        createMockChannel({
          name: 'Channel With Desc',
          description: 'This is a description',
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} compact={true} />);

      await waitFor(() => {
        expect(screen.getByText('Channel With Desc')).toBeInTheDocument();
        expect(screen.queryByText('This is a description')).not.toBeInTheDocument();
      });
    });

    it('shows description in non-compact mode', async () => {
      const channels = [
        createMockChannel({
          name: 'Channel With Desc',
          description: 'This is a description',
        }),
      ];
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue(channels);

      renderWithTheme(<ChannelList {...defaultProps} compact={false} />);

      await waitFor(() => {
        expect(screen.getByText('Channel With Desc')).toBeInTheDocument();
        expect(screen.getByText('This is a description')).toBeInTheDocument();
      });
    });
  });

  describe('create channel', () => {
    it('shows add button in Groups section', async () => {
      vi.mocked(chatService.getUserVisibleChannels).mockResolvedValue([]);

      renderWithTheme(<ChannelList {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Groups')).toBeInTheDocument();
      });

      // Find the Groups section header and its add button
      const groupsSection = screen.getByText('Groups').closest('div');
      expect(groupsSection).toBeInTheDocument();

      // The add button should have a tooltip
      const addButton = screen.getByRole('button', { name: /add groups channel/i });
      expect(addButton).toBeInTheDocument();
    });
  });
});
