/**
 * Chat Service Tests - Teams Integration
 *
 * Tests for Teams connector API methods in chatService.
 * Verifies correct API calls and response handling.
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { chatService } from './chatService';
import { apiClient } from '../../../core/services/api';

// Mock the apiClient
vi.mock('../../../core/services/api', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
    put: vi.fn(),
  },
}));

describe('chatService - Teams Integration', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAvailableTeamsConnectors', () => {
    it('calls correct endpoint with eventId', async () => {
      const mockConnectors = [
        {
          mappingId: 'mapping-1',
          displayName: 'Dev Team',
          conversationId: 'conv-1',
          isLinkedToThisEvent: false,
        },
      ];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockConnectors });

      const result = await chatService.getAvailableTeamsConnectors('event-123');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/events/event-123/chat/available-teams-connectors'
      );
      expect(result).toEqual(mockConnectors);
    });

    it('returns empty array when no connectors', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: [] });

      const result = await chatService.getAvailableTeamsConnectors('event-123');

      expect(result).toEqual([]);
    });

    it('propagates API errors', async () => {
      vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

      await expect(
        chatService.getAvailableTeamsConnectors('event-123')
      ).rejects.toThrow('Network error');
    });
  });

  describe('linkTeamsToChannel', () => {
    it('calls correct endpoint with parameters', async () => {
      const mockChannel = {
        id: 'channel-1',
        eventId: 'event-123',
        name: 'Event Chat',
        channelType: 0,
        externalChannel: {
          id: 'ext-1',
          platform: 3,
          platformName: 'Teams',
        },
      };
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockChannel });

      const result = await chatService.linkTeamsToChannel(
        'event-123',
        'channel-1',
        'teams-conv-456'
      );

      expect(apiClient.post).toHaveBeenCalledWith(
        '/api/events/event-123/chat/channels/channel-1/link-teams',
        { teamsConversationId: 'teams-conv-456' }
      );
      expect(result).toEqual(mockChannel);
    });

    it('returns updated channel with externalChannel populated', async () => {
      const mockChannel = {
        id: 'channel-1',
        eventId: 'event-123',
        name: 'Event Chat',
        channelType: 0,
        channelTypeName: 'Internal',
        isDefaultEventThread: true,
        displayOrder: 0,
        messageCount: 5,
        createdAt: '2025-01-01T00:00:00Z',
        externalChannel: {
          id: 'mapping-1',
          eventId: 'event-123',
          platform: 3,
          platformName: 'Teams',
          externalGroupId: 'teams-conv-456',
          externalGroupName: 'Dev Team',
          isActive: true,
          createdAt: '2025-01-01T00:00:00Z',
        },
      };
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockChannel });

      const result = await chatService.linkTeamsToChannel(
        'event-123',
        'channel-1',
        'teams-conv-456'
      );

      expect(result.externalChannel).toBeDefined();
      expect(result.externalChannel?.platformName).toBe('Teams');
    });

    it('propagates API errors', async () => {
      vi.mocked(apiClient.post).mockRejectedValue(new Error('Channel not found'));

      await expect(
        chatService.linkTeamsToChannel('event-123', 'channel-1', 'teams-conv-456')
      ).rejects.toThrow('Channel not found');
    });
  });

  describe('unlinkExternalChannel', () => {
    it('calls correct endpoint', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await chatService.unlinkExternalChannel('event-123', 'channel-1');

      expect(apiClient.delete).toHaveBeenCalledWith(
        '/api/events/event-123/chat/channels/channel-1/external-link'
      );
    });

    it('propagates API errors', async () => {
      vi.mocked(apiClient.delete).mockRejectedValue(new Error('Channel not found'));

      await expect(
        chatService.unlinkExternalChannel('event-123', 'channel-1')
      ).rejects.toThrow('Channel not found');
    });
  });

  describe('getExternalChannels', () => {
    it('calls correct endpoint', async () => {
      const mockChannels = [
        {
          id: 'ext-1',
          eventId: 'event-123',
          platform: 3,
          platformName: 'Teams',
          externalGroupId: 'teams-1',
          externalGroupName: 'Team A',
          isActive: true,
          createdAt: '2025-01-01T00:00:00Z',
        },
      ];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockChannels });

      const result = await chatService.getExternalChannels('event-123');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/events/event-123/chat/external-channels'
      );
      expect(result).toEqual(mockChannels);
    });
  });

  describe('deactivateExternalChannel', () => {
    it('calls correct endpoint with archive flag false', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await chatService.deactivateExternalChannel('event-123', 'mapping-1', false);

      expect(apiClient.delete).toHaveBeenCalledWith(
        '/api/events/event-123/chat/external-channels/mapping-1?archiveExternalGroup=false'
      );
    });

    it('calls correct endpoint with archive flag true', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await chatService.deactivateExternalChannel('event-123', 'mapping-1', true);

      expect(apiClient.delete).toHaveBeenCalledWith(
        '/api/events/event-123/chat/external-channels/mapping-1?archiveExternalGroup=true'
      );
    });

    it('defaults archive flag to false', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

      await chatService.deactivateExternalChannel('event-123', 'mapping-1');

      expect(apiClient.delete).toHaveBeenCalledWith(
        '/api/events/event-123/chat/external-channels/mapping-1?archiveExternalGroup=false'
      );
    });
  });

  describe('createTeamsChannelMapping (deprecated)', () => {
    it('calls diagnostics endpoint with correct payload', async () => {
      const mockResponse = {
        mappingId: 'mapping-1',
        eventId: 'event-123',
        teamsConversationId: 'teams-conv-1',
        webhookUrl: '/api/webhooks/teams/mapping-1',
        isExisting: false,
      };
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse });

      const result = await chatService.createTeamsChannelMapping(
        'event-123',
        'teams-conv-1',
        'My Team Channel'
      );

      expect(apiClient.post).toHaveBeenCalledWith(
        '/api/chat/diagnostics/teams-mapping',
        {
          eventId: 'event-123',
          teamsConversationId: 'teams-conv-1',
          channelName: 'My Team Channel',
        }
      );

      // Should map response to ExternalChannelMappingDto format
      expect(result.id).toBe('mapping-1');
      expect(result.eventId).toBe('event-123');
      expect(result.platform).toBe(3); // Teams
      expect(result.platformName).toBe('Teams');
    });
  });
});

describe('chatService - Channel Operations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getChannels', () => {
    it('calls correct endpoint', async () => {
      const mockChannels = [
        { id: 'ch-1', name: 'Event Chat', channelType: 0 },
        { id: 'ch-2', name: 'Announcements', channelType: 1 },
      ];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockChannels });

      const result = await chatService.getChannels('event-123');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/events/event-123/chat/channels'
      );
      expect(result).toHaveLength(2);
    });
  });

  describe('getChannel', () => {
    it('calls correct endpoint with channelId', async () => {
      const mockChannel = { id: 'ch-1', name: 'Event Chat' };
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockChannel });

      const result = await chatService.getChannel('event-123', 'ch-1');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/events/event-123/chat/channels/ch-1'
      );
      expect(result.id).toBe('ch-1');
    });
  });

  describe('getUserVisibleChannels', () => {
    it('calls correct endpoint', async () => {
      const mockChannels = [{ id: 'ch-1', name: 'My Position Channel' }];
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockChannels });

      const result = await chatService.getUserVisibleChannels('event-123');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/events/event-123/chat/channels/visible'
      );
      expect(result).toHaveLength(1);
    });
  });
});
