/**
 * ArchiveChannelDialog Component Tests
 *
 * Tests for the archive channel confirmation dialog.
 * Verifies archivability rules and archive workflow.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { ArchiveChannelDialog } from './ArchiveChannelDialog';
import { cobraTheme } from '../../../theme/cobraTheme';
import { chatService } from '../services/chatService';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType } from '../types/chat';

// Mock the chatService
vi.mock('../services/chatService', () => ({
  chatService: {
    deleteChannel: vi.fn(),
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
  messageCount: 10,
  createdAt: new Date().toISOString(),
  ...overrides,
});

describe('ArchiveChannelDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnChannelArchived = vi.fn();

  const defaultProps = {
    open: true,
    onClose: mockOnClose,
    eventId: 'event-1',
    onChannelArchived: mockOnChannelArchived,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders nothing when channel is null', () => {
      const { container } = renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={null} />
      );

      expect(container).toBeEmptyDOMElement();
    });

    it('displays channel name in confirmation message', () => {
      const channel = createMockChannel({ name: 'My Custom Group' });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByText(/My Custom Group/)).toBeInTheDocument();
    });

    it('shows archive consequences info', () => {
      const channel = createMockChannel();
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByText(/hidden from the active channel list/i)).toBeInTheDocument();
      expect(screen.getByText(/messages.*remain accessible/i)).toBeInTheDocument();
      expect(screen.getByText(/can be undone by an administrator/i)).toBeInTheDocument();
    });
  });

  describe('archivability rules', () => {
    it('allows archiving custom channels', () => {
      const channel = createMockChannel({
        channelType: ChannelType.Custom,
        isDefaultEventThread: false,
      });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByRole('button', { name: /Archive Channel/i })).toBeInTheDocument();
    });

    it('allows archiving position channels', () => {
      const channel = createMockChannel({
        channelType: ChannelType.Position,
        isDefaultEventThread: false,
      });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByRole('button', { name: /Archive Channel/i })).toBeInTheDocument();
    });

    it('prevents archiving default event channels', () => {
      const channel = createMockChannel({
        name: 'Event Chat',
        isDefaultEventThread: true,
      });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByText(/Cannot archive this channel/i)).toBeInTheDocument();
      expect(screen.getByText(/Default event channels cannot be archived/i)).toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /Archive Channel/i })).not.toBeInTheDocument();
    });

    it('prevents archiving external channels', () => {
      const channel = createMockChannel({
        name: 'Teams Link',
        channelType: ChannelType.External,
      });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByText(/Cannot archive this channel/i)).toBeInTheDocument();
      expect(screen.getByText(/disconnected instead of archived/i)).toBeInTheDocument();
    });

    it('prevents archiving announcements channel', () => {
      const channel = createMockChannel({
        name: 'Announcements',
        channelType: ChannelType.Announcements,
      });
      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      expect(screen.getByText(/Cannot archive this channel/i)).toBeInTheDocument();
      expect(screen.getByText(/Announcements channel cannot be archived/i)).toBeInTheDocument();
    });
  });

  describe('archive workflow', () => {
    it('calls deleteChannel when archive is confirmed', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel({ id: 'ch-to-archive', name: 'Archive Me' });
      vi.mocked(chatService.deleteChannel).mockResolvedValue(undefined);

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      await waitFor(() => {
        expect(chatService.deleteChannel).toHaveBeenCalledWith('event-1', 'ch-to-archive');
      });
    });

    it('calls onChannelArchived callback on success', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel({ id: 'ch-archived' });
      vi.mocked(chatService.deleteChannel).mockResolvedValue(undefined);

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      await waitFor(() => {
        expect(mockOnChannelArchived).toHaveBeenCalledWith('ch-archived');
      });
    });

    it('closes dialog on successful archive', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();
      vi.mocked(chatService.deleteChannel).mockResolvedValue(undefined);

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('shows loading state during archive', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();
      vi.mocked(chatService.deleteChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      expect(screen.getByRole('button', { name: /Archiving/i })).toBeInTheDocument();
    });

    it('handles archive error', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();
      vi.mocked(chatService.deleteChannel).mockRejectedValue(
        new Error('Archive failed')
      );

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      await waitFor(() => {
        // Dialog should stay open on error
        expect(mockOnClose).not.toHaveBeenCalled();
      });
    });
  });

  describe('dialog controls', () => {
    it('calls onClose when Cancel is clicked', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Cancel/i }));

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('disables Cancel button during archive', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();
      vi.mocked(chatService.deleteChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled();
    });

    it('disables Archive button during archive', async () => {
      const user = userEvent.setup();
      const channel = createMockChannel();
      vi.mocked(chatService.deleteChannel).mockImplementation(
        () => new Promise(() => {})
      );

      renderWithTheme(
        <ArchiveChannelDialog {...defaultProps} channel={channel} />
      );

      await user.click(screen.getByRole('button', { name: /Archive Channel/i }));

      expect(screen.getByRole('button', { name: /Archiving/i })).toBeDisabled();
    });
  });
});
