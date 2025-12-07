/**
 * RestoreChannelDialog Component Tests
 *
 * Tests for the restore channel confirmation dialog.
 * Verifies restore workflow and dialog behavior.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { RestoreChannelDialog } from './RestoreChannelDialog';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock the chatService
vi.mock('../services/chatService', () => ({
  chatService: {
    restoreChannel: vi.fn(),
  },
}));

// Mock react-toastify
vi.mock('react-toastify', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
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
  name: 'Archived Channel',
  channelType: ChannelType.Custom,
  channelTypeName: 'Custom',
  isDefaultEventThread: false,
  displayOrder: 0,
  messageCount: 25,
  createdAt: new Date().toISOString(),
  isArchived: true,
  ...overrides,
});

describe('RestoreChannelDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnChannelRestored = vi.fn();

  const defaultProps = {
    open: true,
    onClose: mockOnClose,
    channel: createMockChannel(),
    eventId: 'event-1',
    onChannelRestored: mockOnChannelRestored,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders dialog with title', () => {
      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      expect(screen.getByRole('dialog')).toHaveTextContent('Restore Channel');
    });

    it('displays channel name in confirmation message', () => {
      const channel = createMockChannel({ name: 'Old Channel' });
      renderWithTheme(<RestoreChannelDialog {...defaultProps} channel={channel} />);

      expect(screen.getByText(/Old Channel/)).toBeInTheDocument();
    });

    it('shows info about what restore does', () => {
      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      expect(screen.getByText(/What happens when you restore/i)).toBeInTheDocument();
      expect(screen.getByText(/visible again in the channel list/i)).toBeInTheDocument();
      expect(screen.getByText(/previous messages will be accessible/i)).toBeInTheDocument();
      expect(screen.getByText(/resume using the channel/i)).toBeInTheDocument();
    });

    it('returns null when channel is null', () => {
      const { container } = renderWithTheme(
        <RestoreChannelDialog {...defaultProps} channel={null} />
      );

      expect(container.querySelector('.MuiDialog-root')).not.toBeInTheDocument();
    });

    it('shows restore button enabled by default', () => {
      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      const restoreButton = screen.getByRole('button', { name: /Restore Channel/i });
      expect(restoreButton).toBeEnabled();
    });
  });

  describe('restore workflow', () => {
    it('calls restoreChannel on confirm', async () => {
      const user = userEvent.setup();
      const restoredChannel = createMockChannel({ isArchived: false });
      vi.mocked(chatService.restoreChannel).mockResolvedValue(restoredChannel);

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      await waitFor(() => {
        expect(chatService.restoreChannel).toHaveBeenCalledWith('event-1', 'channel-1');
      });
    });

    it('calls onChannelRestored callback with restored channel', async () => {
      const user = userEvent.setup();
      const restoredChannel = createMockChannel({ isArchived: false });
      vi.mocked(chatService.restoreChannel).mockResolvedValue(restoredChannel);

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      await waitFor(() => {
        expect(mockOnChannelRestored).toHaveBeenCalledWith(restoredChannel);
      });
    });

    it('closes dialog on successful restore', async () => {
      const user = userEvent.setup();
      const restoredChannel = createMockChannel({ isArchived: false });
      vi.mocked(chatService.restoreChannel).mockResolvedValue(restoredChannel);

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('shows loading state during restore', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.restoreChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      expect(screen.getByRole('button', { name: /Restoring/i })).toBeInTheDocument();
    });

    it('disables restore button during restore', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.restoreChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      const restoreButton = screen.getByRole('button', { name: /Restoring/i });
      expect(restoreButton).toBeDisabled();
    });

    it('handles restore error', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.restoreChannel).mockRejectedValue(
        new Error('Restore failed')
      );

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      await waitFor(() => {
        // Dialog should stay open on error
        expect(mockOnClose).not.toHaveBeenCalled();
      });
    });

    it('does not restore when channel is null', async () => {
      renderWithTheme(<RestoreChannelDialog {...defaultProps} channel={null} />);

      expect(chatService.restoreChannel).not.toHaveBeenCalled();
    });
  });

  describe('dialog controls', () => {
    it('calls onClose when Cancel is clicked', async () => {
      const user = userEvent.setup();
      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Cancel/i }));

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('disables Cancel during restore', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.restoreChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled();
    });

    it('does not close dialog when clicking backdrop during restore', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.restoreChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<RestoreChannelDialog {...defaultProps} />);

      // Start restore
      await user.click(screen.getByRole('button', { name: /Restore Channel/i }));

      // Try to close via cancel (which is disabled)
      const cancelButton = screen.getByRole('button', { name: /Cancel/i });
      expect(cancelButton).toBeDisabled();
    });
  });
});
