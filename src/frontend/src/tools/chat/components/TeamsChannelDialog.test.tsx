/**
 * TeamsChannelDialog Component Tests
 *
 * Tests for the Teams connector linking dialog.
 * Verifies loading states, connector display, selection, and linking workflow.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { TeamsChannelDialog } from './TeamsChannelDialog';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService, type AvailableTeamsConnector } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock the chatService
vi.mock('../services/chatService', () => ({
  chatService: {
    getAvailableTeamsConnectors: vi.fn(),
    linkTeamsToChannel: vi.fn(),
  },
}));

// Helper to render with theme provider
const renderWithTheme = (ui: React.ReactElement) => {
  return render(<ThemeProvider theme={cobraTheme}>{ui}</ThemeProvider>);
};

// Create mock connector
const createMockConnector = (
  overrides: Partial<AvailableTeamsConnector> = {}
): AvailableTeamsConnector => ({
  mappingId: 'mapping-1',
  displayName: 'Test Team Channel',
  conversationId: 'conv-123',
  tenantId: 'tenant-abc',
  lastActivityAt: new Date().toISOString(),
  installedByName: 'Test User',
  createdAt: new Date(Date.now() - 86400000).toISOString(), // 1 day ago
  isLinkedToThisEvent: false,
  linkedChannelName: undefined,
  ...overrides,
});

// Create mock channel
const createMockChannel = (
  overrides: Partial<ChatThreadDto> = {}
): ChatThreadDto => ({
  id: 'channel-1',
  eventId: 'event-1',
  name: 'Event Chat',
  channelType: ChannelType.Internal,
  channelTypeName: 'Internal',
  isDefaultEventThread: true,
  displayOrder: 0,
  messageCount: 10,
  createdAt: new Date().toISOString(),
  ...overrides,
});

describe('TeamsChannelDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnChannelLinked = vi.fn();

  const defaultProps = {
    open: true,
    onClose: mockOnClose,
    eventId: 'event-1',
    targetChannelId: 'channel-1',
    targetChannelName: 'Event Chat',
    onChannelLinked: mockOnChannelLinked,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('loading state', () => {
    it('shows loading spinner while fetching connectors', async () => {
      // Don't resolve the promise yet
      vi.mocked(chatService.getAvailableTeamsConnectors).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      expect(screen.getByText('Loading Teams connectors...')).toBeInTheDocument();
      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });

    it('does not fetch connectors when dialog is closed', () => {
      renderWithTheme(<TeamsChannelDialog {...defaultProps} open={false} />);

      expect(chatService.getAvailableTeamsConnectors).not.toHaveBeenCalled();
    });

    it('fetches connectors when dialog opens', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(chatService.getAvailableTeamsConnectors).toHaveBeenCalledWith('event-1');
      });
    });
  });

  describe('empty state', () => {
    it('shows instructions when no connectors available', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('No Teams connectors available.')).toBeInTheDocument();
      });

      // Should show setup instructions
      expect(screen.getByText(/Install the COBRA bot/i)).toBeInTheDocument();
    });
  });

  describe('error state', () => {
    it('shows error message when fetch fails', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockRejectedValue(
        new Error('Network error')
      );

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Network error')).toBeInTheDocument();
      });
    });

    it('shows generic error for non-Error rejections', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockRejectedValue(
        'Unknown error'
      );

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Failed to load Teams connectors')).toBeInTheDocument();
      });
    });
  });

  describe('connector list', () => {
    it('displays available connectors', async () => {
      const connectors = [
        createMockConnector({ displayName: 'Development Team' }),
        createMockConnector({
          mappingId: 'mapping-2',
          displayName: 'Production Team',
          conversationId: 'conv-456',
        }),
      ];
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue(connectors);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Development Team')).toBeInTheDocument();
        expect(screen.getByText('Production Team')).toBeInTheDocument();
      });
    });

    it('shows connector metadata', async () => {
      const connector = createMockConnector({
        installedByName: 'John Doe',
        lastActivityAt: new Date().toISOString(),
      });
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        // Should show "Installed by" info
        expect(screen.getByText(/Installed by John Doe/)).toBeInTheDocument();
      });
    });

    it('shows "No recent activity" when lastActivityAt is null', async () => {
      const connector = createMockConnector({
        lastActivityAt: undefined,
      });
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText(/No recent activity/)).toBeInTheDocument();
      });
    });

    it('shows linked status for connectors already linked to this event', async () => {
      const connectors = [
        createMockConnector({
          mappingId: 'mapping-linked',
          conversationId: 'conv-linked',
          displayName: 'Linked Connector',
          isLinkedToThisEvent: true,
          linkedChannelName: 'Announcements',
        }),
        createMockConnector({
          mappingId: 'mapping-available',
          conversationId: 'conv-available',
          displayName: 'Available Connector',
          isLinkedToThisEvent: false,
        }),
      ];
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue(connectors);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Linked to Announcements')).toBeInTheDocument();
      });
    });

    it('sorts unlinked connectors before linked ones', async () => {
      const connectors = [
        createMockConnector({
          mappingId: 'mapping-linked',
          conversationId: 'conv-linked-sort',
          displayName: 'A - Linked First',
          isLinkedToThisEvent: true,
          linkedChannelName: 'Some Channel',
        }),
        createMockConnector({
          mappingId: 'mapping-unlinked',
          conversationId: 'conv-unlinked-sort',
          displayName: 'B - Unlinked Second',
          isLinkedToThisEvent: false,
        }),
      ];
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue(connectors);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        const listItems = screen.getAllByRole('button', { name: /Team|Linked|Unlinked/i });
        // Unlinked should appear first despite alphabetical order
        expect(listItems[0]).toHaveTextContent('B - Unlinked Second');
      });
    });

    it('disables linked connectors from selection', async () => {
      const connector = createMockConnector({
        isLinkedToThisEvent: true,
        linkedChannelName: 'Other Channel',
      });
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        const listItem = screen.getByRole('button', { name: /Test Team Channel/i });
        expect(listItem).toHaveAttribute('aria-disabled', 'true');
      });
    });
  });

  describe('connector selection', () => {
    it('allows selecting an unlinked connector', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      const listItem = screen.getByRole('button', { name: /Test Team Channel/i });
      await user.click(listItem);

      // Item should now be selected (has primary border color)
      expect(listItem).toHaveStyle({ borderColor: expect.stringContaining('') });
    });

    it('enables Link button when connector is selected', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      // Link button should be disabled initially
      const linkButton = screen.getByRole('button', { name: /Link Channel/i });
      expect(linkButton).toBeDisabled();

      // Select the connector
      const listItem = screen.getByRole('button', { name: /Test Team Channel/i });
      await user.click(listItem);

      // Link button should now be enabled
      expect(linkButton).toBeEnabled();
    });

    it('disables Link button when no target channel', async () => {
      const connector = createMockConnector();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(
        <TeamsChannelDialog
          {...defaultProps}
          targetChannelId={undefined}
          targetChannelName={undefined}
        />
      );

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      const linkButton = screen.getByRole('button', { name: /Link Channel/i });
      expect(linkButton).toBeDisabled();
    });
  });

  describe('linking workflow', () => {
    it('calls linkTeamsToChannel with correct parameters', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector({ conversationId: 'teams-conv-id' });
      const updatedChannel = createMockChannel({
        externalChannel: {
          id: 'ext-1',
          eventId: 'event-1',
          platform: 3,
          platformName: 'Teams',
          externalGroupId: 'teams-conv-id',
          externalGroupName: 'Test Team Channel',
          isActive: true,
          createdAt: new Date().toISOString(),
        },
      });

      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);
      vi.mocked(chatService.linkTeamsToChannel).mockResolvedValue(updatedChannel);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      // Select connector
      const listItem = screen.getByRole('button', { name: /Test Team Channel/i });
      await user.click(listItem);

      // Click link button
      const linkButton = screen.getByRole('button', { name: /Link Channel/i });
      await user.click(linkButton);

      await waitFor(() => {
        expect(chatService.linkTeamsToChannel).toHaveBeenCalledWith(
          'event-1',
          'channel-1',
          'teams-conv-id'
        );
      });
    });

    it('shows loading state during linking', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();

      // Make linkTeamsToChannel hang
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);
      vi.mocked(chatService.linkTeamsToChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      // Select and link
      await user.click(screen.getByRole('button', { name: /Test Team Channel/i }));
      await user.click(screen.getByRole('button', { name: /Link Channel/i }));

      // Should show "Linking..." on button
      expect(screen.getByRole('button', { name: /Linking/i })).toBeInTheDocument();
    });

    it('calls onChannelLinked callback on success', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();
      const updatedChannel = createMockChannel();

      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);
      vi.mocked(chatService.linkTeamsToChannel).mockResolvedValue(updatedChannel);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /Test Team Channel/i }));
      await user.click(screen.getByRole('button', { name: /Link Channel/i }));

      await waitFor(() => {
        expect(mockOnChannelLinked).toHaveBeenCalledWith(updatedChannel);
      });
    });

    it('closes dialog on successful link', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();
      const updatedChannel = createMockChannel();

      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);
      vi.mocked(chatService.linkTeamsToChannel).mockResolvedValue(updatedChannel);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /Test Team Channel/i }));
      await user.click(screen.getByRole('button', { name: /Link Channel/i }));

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('shows error message on link failure', async () => {
      const user = userEvent.setup();
      const connector = createMockConnector();

      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);
      vi.mocked(chatService.linkTeamsToChannel).mockRejectedValue(
        new Error('Channel already linked')
      );

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /Test Team Channel/i }));
      await user.click(screen.getByRole('button', { name: /Link Channel/i }));

      await waitFor(() => {
        expect(screen.getByText('Channel already linked')).toBeInTheDocument();
      });

      // Dialog should stay open
      expect(mockOnClose).not.toHaveBeenCalled();
    });
  });

  describe('dialog UI', () => {
    it('displays target channel name in title', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([]);

      renderWithTheme(
        <TeamsChannelDialog {...defaultProps} targetChannelName="Announcements" />
      );

      await waitFor(() => {
        expect(screen.getByText('Link Teams to "Announcements"')).toBeInTheDocument();
      });
    });

    it('uses default channel name when not provided', async () => {
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([]);

      renderWithTheme(
        <TeamsChannelDialog {...defaultProps} targetChannelName={undefined} />
      );

      await waitFor(() => {
        expect(screen.getByText('Link Teams to "Channel"')).toBeInTheDocument();
      });
    });

    it('calls onClose when Cancel is clicked', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Cancel')).toBeInTheDocument();
      });

      await user.click(screen.getByText('Cancel'));

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('shows bidirectional sync info when connectors available', async () => {
      const connector = createMockConnector();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(
          screen.getByText(/Messages sent in this channel will appear in Teams/i)
        ).toBeInTheDocument();
      });
    });

    it('resets selection when dialog reopens', async () => {
      const connector = createMockConnector();
      vi.mocked(chatService.getAvailableTeamsConnectors).mockResolvedValue([connector]);

      const { rerender } = renderWithTheme(<TeamsChannelDialog {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Test Team Channel')).toBeInTheDocument();
      });

      // Close dialog
      rerender(
        <ThemeProvider theme={cobraTheme}>
          <TeamsChannelDialog {...defaultProps} open={false} />
        </ThemeProvider>
      );

      // Reopen dialog
      rerender(
        <ThemeProvider theme={cobraTheme}>
          <TeamsChannelDialog {...defaultProps} open={true} />
        </ThemeProvider>
      );

      await waitFor(() => {
        // Link button should be disabled (no selection)
        const linkButton = screen.getByRole('button', { name: /Link Channel/i });
        expect(linkButton).toBeDisabled();
      });
    });
  });
});
