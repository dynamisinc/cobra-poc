/**
 * Chat Page
 *
 * Full-page chat view with tabbed channel navigation.
 * Displays all event channels with support for external platform
 * integration (GroupMe, etc.)
 *
 * Route: /chat
 * Breadcrumb: Home / Events / [Event Name] / Chat
 *
 * Related User Stories:
 * - UC-001: Auto-Create Default Channels (channel tabs)
 * - UC-014: Full-page chat view with tabbed channels
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  Stack,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Tabs,
  Tab,
  Skeleton,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faComments,
  faExclamationTriangle,
  faBullhorn,
  faHashtag,
  faUserGroup,
} from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';
import { useEvents } from '../../../shared/events';
import { EventChat } from '../components/EventChat';
import { chatService } from '../services/chatService';
import CobraStyles from '../../../theme/CobraStyles';
import { CobraPrimaryButton } from '../../../theme/styledComponents';
import { useTheme, Theme } from '@mui/material/styles';
import type { ChatThreadDto } from '../types/chat';
import { ChannelType, ExternalPlatform, PlatformInfo } from '../types/chat';

/**
 * Get icon for channel based on type
 */
const getChannelIcon = (channel: ChatThreadDto) => {
  if (channel.iconName) {
    switch (channel.iconName) {
      case 'comments':
        return faComments;
      case 'bullhorn':
        return faBullhorn;
      case 'hashtag':
        return faHashtag;
      case 'user-group':
        return faUserGroup;
      default:
        return faHashtag;
    }
  }

  switch (channel.channelType) {
    case ChannelType.Internal:
      return faComments;
    case ChannelType.Announcements:
      return faBullhorn;
    case ChannelType.Position:
      return faUserGroup;
    case ChannelType.External:
    case ChannelType.Custom:
    default:
      return faHashtag;
  }
};

/**
 * Get color for channel tab
 */
const getChannelColor = (channel: ChatThreadDto, theme: Theme) => {
  if (channel.color) {
    return channel.color;
  }

  if (channel.channelType === ChannelType.External && channel.externalChannel) {
    const platformKey = channel.externalChannel.platform as ExternalPlatform;
    const platformInfo = PlatformInfo[platformKey];
    if (platformInfo) {
      return platformInfo.color;
    }
  }

  switch (channel.channelType) {
    case ChannelType.Internal:
      return theme.palette.primary.main;
    case ChannelType.Announcements:
      return theme.palette.warning.main;
    case ChannelType.Position:
      return theme.palette.info.main;
    case ChannelType.Custom:
    default:
      return theme.palette.text.secondary;
  }
};

/**
 * Chat Page Component
 */
export const ChatPage: React.FC = () => {
  const navigate = useNavigate();
  const theme = useTheme();
  const { currentEvent, loading: eventLoading } = useEvents();

  // Channel state
  const [channels, setChannels] = useState<ChatThreadDto[]>([]);
  const [selectedChannelId, setSelectedChannelId] = useState<string | null>(null);
  const [channelsLoading, setChannelsLoading] = useState(true);
  const [channelsError, setChannelsError] = useState<string | null>(null);

  // Load channels when event changes
  const loadChannels = useCallback(async () => {
    if (!currentEvent) return;

    try {
      setChannelsLoading(true);
      setChannelsError(null);
      const data = await chatService.getChannels(currentEvent.id);
      setChannels(data);

      // Select first channel by default
      if (data.length > 0 && !selectedChannelId) {
        setSelectedChannelId(data[0].id);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load channels';
      setChannelsError(message);
      console.error('Failed to load channels:', err);
    } finally {
      setChannelsLoading(false);
    }
  }, [currentEvent, selectedChannelId]);

  useEffect(() => {
    loadChannels();
  }, [loadChannels]);

  // Reset selection when event changes
  useEffect(() => {
    setSelectedChannelId(null);
    setChannels([]);
  }, [currentEvent?.id]);

  // Get selected channel
  const selectedChannel = channels.find((c) => c.id === selectedChannelId) || null;

  // Handle tab change
  const handleTabChange = (_event: React.SyntheticEvent, newValue: string) => {
    setSelectedChannelId(newValue);
  };

  if (eventLoading) {
    return (
      <Container maxWidth={false} disableGutters>
        <Stack
          spacing={2}
          padding={CobraStyles.Padding.MainWindow}
          alignItems="center"
          justifyContent="center"
          sx={{ minHeight: '50vh' }}
        >
          <CircularProgress />
          <Typography>Loading...</Typography>
        </Stack>
      </Container>
    );
  }

  if (!currentEvent) {
    return (
      <Container maxWidth={false} disableGutters>
        <Stack spacing={3} padding={CobraStyles.Padding.MainWindow}>
          <Alert severity="warning" icon={<FontAwesomeIcon icon={faExclamationTriangle} />}>
            Please select an event to access the chat.
          </Alert>
          <Box>
            <CobraPrimaryButton onClick={() => navigate('/events')}>
              Go to Events
            </CobraPrimaryButton>
          </Box>
        </Stack>
      </Container>
    );
  }

  return (
    <Container maxWidth={false} disableGutters>
      <Stack spacing={3} padding={CobraStyles.Padding.MainWindow}>
        {/* Page Header */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              backgroundColor: theme.palette.buttonPrimary.light,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <FontAwesomeIcon
              icon={faComments}
              size="lg"
              style={{ color: theme.palette.buttonPrimary.main }}
            />
          </Box>
          <Box>
            <Typography variant="h5" fontWeight={600}>
              Event Chat
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {currentEvent.name}
            </Typography>
          </Box>
        </Box>

        {/* Info Card */}
        <Card
          variant="outlined"
          sx={{
            backgroundColor: theme.palette.info.light,
            borderColor: theme.palette.info.main,
          }}
        >
          <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
            <Typography variant="body2">
              Chat with your team in real-time. Connect external channels like GroupMe
              to include field personnel who don't have access to COBRA.
            </Typography>
          </CardContent>
        </Card>

        {/* Channel Tabs and Chat */}
        <Box sx={{ maxWidth: 900 }}>
          {/* Channel Tabs */}
          {channelsLoading ? (
            <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} variant="rounded" width={120} height={36} />
              ))}
            </Box>
          ) : channelsError ? (
            <Alert severity="error" sx={{ mb: 2 }}>
              {channelsError}
            </Alert>
          ) : channels.length > 0 ? (
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
              <Tabs
                value={selectedChannelId || false}
                onChange={handleTabChange}
                variant="scrollable"
                scrollButtons="auto"
                sx={{
                  '& .MuiTab-root': {
                    textTransform: 'none',
                    minHeight: 48,
                    fontWeight: 500,
                  },
                }}
              >
                {channels.map((channel) => {
                  const icon = getChannelIcon(channel);
                  const color = getChannelColor(channel, theme);
                  const isSelected = channel.id === selectedChannelId;

                  return (
                    <Tab
                      key={channel.id}
                      value={channel.id}
                      label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <FontAwesomeIcon
                            icon={icon}
                            style={{
                              fontSize: 14,
                              color: isSelected ? color : theme.palette.text.secondary,
                            }}
                          />
                          <span>{channel.name}</span>
                        </Box>
                      }
                      sx={{
                        '&.Mui-selected': {
                          color: color,
                        },
                      }}
                    />
                  );
                })}
              </Tabs>
            </Box>
          ) : (
            <Alert severity="info" sx={{ mb: 2 }}>
              No channels available for this event.
            </Alert>
          )}

          {/* Chat Component */}
          {selectedChannel ? (
            <EventChat
              eventId={currentEvent.id}
              eventName={currentEvent.name}
              channelId={selectedChannel.id}
              channelName={selectedChannel.name}
              channelType={selectedChannel.channelType}
            />
          ) : !channelsLoading && channels.length === 0 ? (
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: 300,
                border: `1px dashed ${theme.palette.divider}`,
                borderRadius: 1,
              }}
            >
              <Typography variant="body2" color="text.secondary">
                No channels to display
              </Typography>
            </Box>
          ) : null}
        </Box>
      </Stack>
    </Container>
  );
};

export default ChatPage;
