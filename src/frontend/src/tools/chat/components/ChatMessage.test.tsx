/**
 * ChatMessage Component Tests
 *
 * Tests for the chat message display component.
 * Verifies rendering for own messages, other users' messages, and external messages.
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider } from '@mui/material/styles';
import { ChatMessage } from './ChatMessage';
import { cobraTheme } from '../../../theme/cobraTheme';
import type { ChatMessageDto } from '../types/chat';

// Helper to render with theme provider
const renderWithTheme = (ui: React.ReactElement) => {
  return render(<ThemeProvider theme={cobraTheme}>{ui}</ThemeProvider>);
};

// Create mock message
const createMockMessage = (
  overrides: Partial<ChatMessageDto> = {}
): ChatMessageDto => ({
  id: 'msg-1',
  chatThreadId: 'thread-1',
  createdAt: '2025-01-15T10:30:00Z',
  createdBy: 'user@example.com',
  senderDisplayName: 'John Doe',
  message: 'Hello, world!',
  isExternalMessage: false,
  ...overrides,
});

describe('ChatMessage', () => {
  describe('own messages', () => {
    it('renders own message right-aligned', () => {
      const message = createMockMessage({ message: 'My own message' });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={true} />);

      expect(screen.getByText('My own message')).toBeInTheDocument();
    });

    it('displays timestamp for own messages', () => {
      const message = createMockMessage({
        createdAt: '2025-01-15T10:30:00Z',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={true} />);

      // Timestamp should be displayed (format may vary by locale)
      expect(screen.getByText(/1\/15\/2025/)).toBeInTheDocument();
    });

    it('applies cyan color to own message text', () => {
      const message = createMockMessage({ message: 'Cyan message' });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={true} />);

      const messageText = screen.getByText('Cyan message');
      expect(messageText).toHaveStyle({ color: '#00BCD4' });
    });

    it('does not show avatar for own messages', () => {
      const message = createMockMessage({ senderDisplayName: 'John Doe' });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={true} />);

      // Avatar would show initials "JD" - should not be present
      expect(screen.queryByText('JD')).not.toBeInTheDocument();
    });
  });

  describe('other users messages', () => {
    it('renders other user message with avatar', () => {
      const message = createMockMessage({
        senderDisplayName: 'Jane Smith',
        message: 'Hello from Jane',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText('Hello from Jane')).toBeInTheDocument();
      // Avatar shows initials
      expect(screen.getByText('JS')).toBeInTheDocument();
    });

    it('displays sender name and timestamp', () => {
      const message = createMockMessage({
        senderDisplayName: 'Bob Johnson',
        createdAt: '2025-01-15T14:45:00Z',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText(/Bob Johnson/)).toBeInTheDocument();
      expect(screen.getByText(/1\/15\/2025/)).toBeInTheDocument();
    });

    it('handles single-word names for initials', () => {
      const message = createMockMessage({
        senderDisplayName: 'Admin',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText('A')).toBeInTheDocument();
    });

    it('handles empty name gracefully', () => {
      const message = createMockMessage({
        senderDisplayName: '',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      // Should show '?' for empty name
      expect(screen.getByText('?')).toBeInTheDocument();
    });

    it('strips parenthetical suffixes from initials', () => {
      const message = createMockMessage({
        senderDisplayName: 'John Doe (via Teams)',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      // Should show "JD" not "J(" or similar
      expect(screen.getByText('JD')).toBeInTheDocument();
    });
  });

  describe('external messages', () => {
    it('renders external message with platform suffix', () => {
      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'Teams',
        externalSenderName: 'External User',
        message: 'Message from Teams',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText('Message from Teams')).toBeInTheDocument();
      expect(screen.getByText(/External User/)).toBeInTheDocument();
      expect(screen.getByText(/via Teams/)).toBeInTheDocument();
    });

    it('uses external sender name over display name', () => {
      const message = createMockMessage({
        senderDisplayName: 'COBRA User',
        isExternalMessage: true,
        externalSource: 'GroupMe',
        externalSenderName: 'GroupMe User',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText(/GroupMe User/)).toBeInTheDocument();
      // Should show initials for external name
      expect(screen.getByText('GU')).toBeInTheDocument();
    });

    it('falls back to Unknown for missing external sender', () => {
      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'Teams',
        externalSenderName: undefined,
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.getByText(/Unknown/)).toBeInTheDocument();
    });

    it('shows tooltip for external messages', async () => {
      const user = userEvent.setup();
      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'Teams',
        externalSenderName: 'Teams User',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      // Hover over avatar area to trigger tooltip
      const avatar = screen.getByText('TU');
      await user.hover(avatar);

      // Tooltip should appear
      expect(await screen.findByText(/External user via Teams/)).toBeInTheDocument();
    });

    it('shows COBRA user tooltip for internal messages', async () => {
      const user = userEvent.setup();
      const message = createMockMessage({
        isExternalMessage: false,
        senderDisplayName: 'Internal User',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      const avatar = screen.getByText('IU');
      await user.hover(avatar);

      expect(await screen.findByText('COBRA user')).toBeInTheDocument();
    });
  });

  describe('attachments', () => {
    it('renders image attachment', () => {
      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'GroupMe',
        externalSenderName: 'Photo User',
        externalAttachmentUrl: 'https://example.com/image.jpg',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      const image = screen.getByAltText('Attached image');
      expect(image).toBeInTheDocument();
      expect(image).toHaveAttribute('src', 'https://example.com/image.jpg');
    });

    it('opens image in new tab when clicked', async () => {
      const user = userEvent.setup();
      const windowOpenSpy = vi.spyOn(window, 'open').mockImplementation(() => null);

      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'GroupMe',
        externalSenderName: 'Photo User',
        externalAttachmentUrl: 'https://example.com/photo.png',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      const image = screen.getByAltText('Attached image');
      await user.click(image);

      expect(windowOpenSpy).toHaveBeenCalledWith('https://example.com/photo.png', '_blank');
      windowOpenSpy.mockRestore();
    });

    it('does not render image when no attachment URL', () => {
      const message = createMockMessage({
        externalAttachmentUrl: undefined,
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      expect(screen.queryByAltText('Attached image')).not.toBeInTheDocument();
    });
  });

  describe('message formatting', () => {
    it('preserves whitespace in messages', () => {
      const message = createMockMessage({
        message: 'Line 1\nLine 2\n\nLine 4',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      const messageElement = screen.getByText(/Line 1/);
      expect(messageElement).toHaveStyle({ whiteSpace: 'pre-wrap' });
    });

    it('allows word breaking for long messages', () => {
      const message = createMockMessage({
        message: 'VeryLongWordWithoutSpacesThatShouldBreak',
      });
      renderWithTheme(<ChatMessage message={message} isOwnMessage={false} />);

      const messageElement = screen.getByText(/VeryLongWord/);
      expect(messageElement).toHaveStyle({ wordBreak: 'break-word' });
    });
  });

  describe('avatar colors', () => {
    it('generates consistent color for same name', () => {
      const message1 = createMockMessage({ senderDisplayName: 'Consistent User' });
      const message2 = createMockMessage({ senderDisplayName: 'Consistent User' });

      const { container: container1 } = renderWithTheme(
        <ChatMessage message={message1} isOwnMessage={false} />
      );
      const { container: container2 } = renderWithTheme(
        <ChatMessage message={message2} isOwnMessage={false} />
      );

      // Get the avatar background colors
      const avatar1 = container1.querySelector('.MuiAvatar-root');
      const avatar2 = container2.querySelector('.MuiAvatar-root');

      expect(avatar1).toBeInTheDocument();
      expect(avatar2).toBeInTheDocument();

      // Both should have the same background color
      const style1 = window.getComputedStyle(avatar1!);
      const style2 = window.getComputedStyle(avatar2!);
      expect(style1.backgroundColor).toBe(style2.backgroundColor);
    });

    it('uses platform color for external messages', () => {
      const message = createMockMessage({
        isExternalMessage: true,
        externalSource: 'Teams',
        externalSenderName: 'Teams User',
      });

      const { container } = renderWithTheme(
        <ChatMessage message={message} isOwnMessage={false} />
      );

      const avatar = container.querySelector('.MuiAvatar-root');
      expect(avatar).toBeInTheDocument();
      // Teams color is #6264a7
    });
  });
});
