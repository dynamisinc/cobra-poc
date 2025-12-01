/**
 * Feature Flags Admin Component
 *
 * Admin UI for managing feature flags.
 * Allows toggling which POC tools are enabled/visible.
 */

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Chip,
  IconButton,
  Tooltip,
  CircularProgress,
  Alert,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faRotateLeft,
  faClipboardList,
  faComments,
  faListCheck,
  faBrain,
  faFileLines,
  faTableCells,
  faTimeline,
  faRobot,
} from '@fortawesome/free-solid-svg-icons';
import type { IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { CobraSwitch, CobraSecondaryButton } from '../../theme/styledComponents';
import CobraStyles from '../../theme/CobraStyles';
import { useFeatureFlags } from '../../contexts/FeatureFlagsContext';
import { FeatureFlags, featureFlagInfo } from '../../types/featureFlags';

// Map flag keys to icons
const flagIcons: Record<keyof FeatureFlags, IconDefinition> = {
  checklist: faClipboardList,
  chat: faComments,
  tasking: faListCheck,
  cobraKai: faBrain,
  eventSummary: faFileLines,
  statusChart: faTableCells,
  eventTimeline: faTimeline,
  cobraAi: faRobot,
};

// Map categories to display names
const categoryLabels: Record<string, string> = {
  core: 'Core Tools',
  communication: 'Communication',
  visualization: 'Visualization',
  ai: 'AI & Intelligence',
};

// Map categories to colors
const categoryColors: Record<string, 'primary' | 'secondary' | 'success' | 'info'> = {
  core: 'primary',
  communication: 'info',
  visualization: 'success',
  ai: 'secondary',
};

export const FeatureFlagsAdmin: React.FC = () => {
  const theme = useTheme();
  const { flags, loading, error, updateFlags, resetFlags } = useFeatureFlags();
  const [saving, setSaving] = useState(false);

  const handleToggle = async (flagKey: keyof FeatureFlags) => {
    try {
      setSaving(true);
      const newFlags = { ...flags, [flagKey]: !flags[flagKey] };
      await updateFlags(newFlags);
    } catch (err) {
      // Error is handled by context
    } finally {
      setSaving(false);
    }
  };

  const handleReset = async () => {
    try {
      setSaving(true);
      await resetFlags();
    } catch (err) {
      // Error is handled by context
    } finally {
      setSaving(false);
    }
  };

  if (loading && !flags) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        Failed to load feature flags: {error}
      </Alert>
    );
  }

  // Group flags by category
  const flagsByCategory = featureFlagInfo.reduce((acc, info) => {
    if (!acc[info.category]) {
      acc[info.category] = [];
    }
    acc[info.category].push(info);
    return acc;
  }, {} as Record<string, typeof featureFlagInfo>);

  return (
    <Box>
      {/* Header with reset button */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Toggle tools on/off to control what users see during POC evaluation.
          Changes are saved immediately and apply to all users.
        </Typography>
        <Tooltip title="Reset all flags to appsettings.json defaults">
          <span>
            <CobraSecondaryButton
              onClick={handleReset}
              disabled={saving}
              size="small"
              startIcon={<FontAwesomeIcon icon={faRotateLeft} />}
            >
              Reset to Defaults
            </CobraSecondaryButton>
          </span>
        </Tooltip>
      </Box>

      {/* Flag cards by category */}
      {Object.entries(flagsByCategory).map(([category, categoryFlags]) => (
        <Box key={category} sx={{ mb: 3 }}>
          <Typography
            variant="subtitle2"
            sx={{ mb: 1, color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: 1 }}
          >
            {categoryLabels[category] || category}
          </Typography>
          <Grid container spacing={2}>
            {categoryFlags.map((info) => {
              const isEnabled = flags[info.key];
              return (
                <Grid item xs={12} sm={6} md={4} key={info.key}>
                  <Card
                    sx={{
                      height: '100%',
                      opacity: isEnabled ? 1 : 0.7,
                      borderLeft: isEnabled
                        ? `4px solid ${theme.palette.success.main}`
                        : `4px solid ${theme.palette.grey[300]}`,
                      transition: 'all 0.2s ease',
                    }}
                  >
                    <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
                      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
                        {/* Icon */}
                        <Box
                          sx={{
                            width: 36,
                            height: 36,
                            borderRadius: 1,
                            backgroundColor: isEnabled
                              ? theme.palette.buttonPrimary.light
                              : theme.palette.grey[100],
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            flexShrink: 0,
                          }}
                        >
                          <FontAwesomeIcon
                            icon={flagIcons[info.key]}
                            style={{
                              color: isEnabled
                                ? theme.palette.buttonPrimary.main
                                : theme.palette.grey[400],
                            }}
                          />
                        </Box>

                        {/* Content */}
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                            <Typography variant="subtitle2" noWrap>
                              {info.name}
                            </Typography>
                            <Chip
                              label={isEnabled ? 'On' : 'Off'}
                              size="small"
                              color={isEnabled ? 'success' : 'default'}
                              sx={{ height: 20, fontSize: 11 }}
                            />
                          </Box>
                          <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{
                              display: '-webkit-box',
                              WebkitLineClamp: 2,
                              WebkitBoxOrient: 'vertical',
                              overflow: 'hidden',
                            }}
                          >
                            {info.description}
                          </Typography>
                        </Box>

                        {/* Toggle */}
                        <CobraSwitch
                          checked={isEnabled}
                          onChange={() => handleToggle(info.key)}
                          disabled={saving}
                          size="small"
                        />
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              );
            })}
          </Grid>
        </Box>
      ))}

      {saving && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
          <CircularProgress size={24} />
        </Box>
      )}
    </Box>
  );
};

export default FeatureFlagsAdmin;
