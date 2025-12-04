/**
 * TeamsChannelDialog Component
 *
 * A dialog for selecting and connecting a Teams channel to a COBRA event.
 * Shows available Teams conversations where the bot is installed.
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  List,
  ListItemButton,
  ListItemText,
  ListItemIcon,
  CircularProgress,
  Alert,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMicrosoft } from '@fortawesome/free-brands-svg-icons';
import { CobraDialog, CobraPrimaryButton, CobraLinkButton } from '../../../theme/styledComponents';
import { chatService, type TeamsConversation } from '../services/chatService';
import type { ExternalChannelMappingDto } from '../types/chat';

interface TeamsChannelDialogProps {
  open: boolean;
  onClose: () => void;
  eventId: string;
  onChannelConnected: (channel: ExternalChannelMappingDto) => void;
}

export const TeamsChannelDialog: React.FC<TeamsChannelDialogProps> = ({
  open,
  onClose,
  eventId,
  onChannelConnected,
}) => {
  const [conversations, setConversations] = useState<TeamsConversation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null);
  const [connecting, setConnecting] = useState(false);

  // Load available Teams conversations when dialog opens
  useEffect(() => {
    if (!open) return;

    const loadConversations = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await chatService.getTeamsConversations();
        setConversations(data);
        setSelectedConversation(null);
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Failed to load Teams channels';
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    loadConversations();
  }, [open]);

  const handleConnect = async () => {
    if (!selectedConversation) return;

    const conversation = conversations.find(c => c.conversationId === selectedConversation);
    if (!conversation) return;

    setConnecting(true);
    try {
      const channel = await chatService.createTeamsChannelMapping(
        eventId,
        conversation.conversationId,
        `Teams: ${conversation.channelId}`
      );
      onChannelConnected(channel);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to connect Teams channel';
      setError(message);
    } finally {
      setConnecting(false);
    }
  };

  return (
    <CobraDialog
      open={open}
      onClose={onClose}
      title="Connect Teams Channel"
      contentWidth="500px"
    >
      <Box sx={{ minHeight: 200 }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 200 }}>
            <CircularProgress size={32} />
            <Typography sx={{ ml: 2 }} color="text.secondary">
              Loading Teams channels...
            </Typography>
          </Box>
        ) : error ? (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        ) : conversations.length === 0 ? (
          <Alert severity="info">
            <Typography variant="body2" gutterBottom>
              No Teams channels available.
            </Typography>
            <Typography variant="body2" color="text.secondary">
              To connect a Teams channel:
              <ol style={{ margin: '8px 0', paddingLeft: '20px' }}>
                <li>Install the COBRA bot in your Teams channel</li>
                <li>Send a message in the channel to activate the bot</li>
                <li>Return here to link the channel</li>
              </ol>
            </Typography>
          </Alert>
        ) : (
          <>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Select a Teams channel to connect to this event:
            </Typography>
            <List sx={{ bgcolor: 'background.paper', borderRadius: 1 }}>
              {conversations.map((conv) => (
                <ListItemButton
                  key={conv.conversationId}
                  selected={selectedConversation === conv.conversationId}
                  onClick={() => setSelectedConversation(conv.conversationId)}
                  sx={{
                    border: '1px solid',
                    borderColor: selectedConversation === conv.conversationId
                      ? 'primary.main'
                      : 'divider',
                    borderRadius: 1,
                    mb: 1,
                  }}
                >
                  <ListItemIcon>
                    <FontAwesomeIcon
                      icon={faMicrosoft}
                      style={{ fontSize: 20, color: '#6264a7' }}
                    />
                  </ListItemIcon>
                  <ListItemText
                    primary={conv.channelId === 'msteams' ? 'Teams Conversation' : conv.channelId}
                    secondary={
                      <Typography variant="caption" component="span" color="text.secondary">
                        {conv.conversationId.substring(0, 30)}...
                      </Typography>
                    }
                  />
                </ListItemButton>
              ))}
            </List>
          </>
        )}
      </Box>

      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 2 }}>
        <CobraLinkButton onClick={onClose}>
          Cancel
        </CobraLinkButton>
        <CobraPrimaryButton
          onClick={handleConnect}
          disabled={!selectedConversation || connecting}
        >
          {connecting ? 'Connecting...' : 'Connect Channel'}
        </CobraPrimaryButton>
      </Box>
    </CobraDialog>
  );
};

export default TeamsChannelDialog;
