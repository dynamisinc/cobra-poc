/**
 * PermanentDeleteDialog Component Tests
 *
 * Tests for the permanent delete confirmation dialog.
 * Verifies confirmation text requirement and delete workflow.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { PermanentDeleteDialog } from './PermanentDeleteDialog';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock the chatService
vi.mock('../services/chatService', () => ({
  chatService: {
    permanentDeleteChannel: vi.fn(),
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
  name: 'Test Channel',
  channelType: ChannelType.Custom,
  channelTypeName: 'Custom',
  isDefaultEventThread: false,
  displayOrder: 0,
  messageCount: 42,
  createdAt: new Date().toISOString(),
  ...overrides,
});

describe('PermanentDeleteDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnChannelDeleted = vi.fn();

  const defaultProps = {
    open: true,
    onClose: mockOnClose,
    channel: createMockChannel(),
    eventId: 'event-1',
    onChannelDeleted: mockOnChannelDeleted,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders dialog with title', () => {
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      expect(screen.getByText('Permanently Delete Channel')).toBeInTheDocument();
    });

    it('displays channel name in warning', () => {
      const channel = createMockChannel({ name: 'Important Channel' });
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} channel={channel} />);

      expect(screen.getByText(/Important Channel/)).toBeInTheDocument();
    });

    it('displays message count in warning', () => {
      const channel = createMockChannel({ messageCount: 150 });
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} channel={channel} />);

      expect(screen.getByText(/150 messages/)).toBeInTheDocument();
    });

    it('shows warning about irreversible action', () => {
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      expect(screen.getByText('This action cannot be undone!')).toBeInTheDocument();
    });

    it('shows confirmation text requirement', () => {
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      expect(screen.getByText(/type/i)).toBeInTheDocument();
      expect(screen.getByText('DELETE')).toBeInTheDocument();
    });

    it('returns null when channel is null', () => {
      const { container } = renderWithTheme(
        <PermanentDeleteDialog {...defaultProps} channel={null} />
      );

      expect(container.querySelector('.MuiDialog-root')).not.toBeInTheDocument();
    });
  });

  describe('confirmation text', () => {
    it('disables delete button when confirmation text is empty', () => {
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const deleteButton = screen.getByRole('button', { name: /Delete Permanently/i });
      expect(deleteButton).toBeDisabled();
    });

    it('disables delete button when confirmation text is incorrect', async () => {
      const user = userEvent.setup();
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'WRONG');

      const deleteButton = screen.getByRole('button', { name: /Delete Permanently/i });
      expect(deleteButton).toBeDisabled();
    });

    it('enables delete button when confirmation text matches exactly', async () => {
      const user = userEvent.setup();
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');

      const deleteButton = screen.getByRole('button', { name: /Delete Permanently/i });
      expect(deleteButton).toBeEnabled();
    });

    it('accepts lowercase confirmation text', async () => {
      const user = userEvent.setup();
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'delete');

      const deleteButton = screen.getByRole('button', { name: /Delete Permanently/i });
      expect(deleteButton).toBeEnabled();
    });

    it('accepts mixed case confirmation text', async () => {
      const user = userEvent.setup();
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DeLeTe');

      const deleteButton = screen.getByRole('button', { name: /Delete Permanently/i });
      expect(deleteButton).toBeEnabled();
    });
  });

  describe('delete workflow', () => {
    it('calls permanentDeleteChannel on confirm', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockResolvedValue(undefined);

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      await waitFor(() => {
        expect(chatService.permanentDeleteChannel).toHaveBeenCalledWith('event-1', 'channel-1');
      });
    });

    it('calls onChannelDeleted callback on success', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockResolvedValue(undefined);

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      await waitFor(() => {
        expect(mockOnChannelDeleted).toHaveBeenCalledWith('channel-1');
      });
    });

    it('closes dialog on successful delete', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockResolvedValue(undefined);

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('shows loading state during delete', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      expect(screen.getByRole('button', { name: /Deleting/i })).toBeInTheDocument();
    });

    it('handles delete error', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockRejectedValue(
        new Error('Delete failed')
      );

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      await waitFor(() => {
        // Dialog should stay open on error
        expect(mockOnClose).not.toHaveBeenCalled();
      });
    });

    it('does not delete when channel is null', async () => {
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} channel={null} />);

      expect(chatService.permanentDeleteChannel).not.toHaveBeenCalled();
    });
  });

  describe('dialog controls', () => {
    it('calls onClose when Cancel is clicked', async () => {
      const user = userEvent.setup();
      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /Cancel/i }));

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('disables Cancel during delete', async () => {
      const user = userEvent.setup();
      vi.mocked(chatService.permanentDeleteChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(<PermanentDeleteDialog {...defaultProps} />);

      const input = screen.getByPlaceholderText('DELETE');
      await user.type(input, 'DELETE');
      await user.click(screen.getByRole('button', { name: /Delete Permanently/i }));

      expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled();
    });
  });
});
