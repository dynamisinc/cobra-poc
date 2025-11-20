/**
 * ChecklistCard Component
 *
 * Displays a single checklist instance as a card with:
 * - Fixed height (280px) for grid consistency
 * - Template type indicator (recurring, one-time, urgent)
 * - Progress bar with C5 color coding
 * - Text ellipsis for long content
 * - Assigned positions with overflow handling
 */

import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  Typography,
  Box,
  LinearProgress,
  Chip,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faArrowsRotate,
  faClipboardList,
  faExclamationTriangle,
} from '@fortawesome/free-solid-svg-icons';
import type { ChecklistInstanceDto } from '../services/checklistService';
import { c5Colors } from '../theme/c5Theme';

/**
 * Template type for visual indicators
 */
type TemplateType = 'recurring' | 'one-time' | 'urgent';

/**
 * Props for ChecklistCard
 */
interface ChecklistCardProps {
  checklist: ChecklistInstanceDto;
  templateType?: TemplateType; // Optional: inferred from name patterns or explicit field
}

/**
 * Get progress bar color based on completion percentage
 */
const getProgressColor = (percentage: number): string => {
  if (percentage === 100) return c5Colors.successGreen;
  if (percentage >= 67) return c5Colors.cobaltBlue;
  if (percentage >= 34) return c5Colors.canaryYellow;
  return c5Colors.lavaRed;
};

/**
 * Get template type icon and color
 */
const getTemplateIndicator = (type: TemplateType) => {
  switch (type) {
    case 'recurring':
      return {
        icon: faArrowsRotate,
        color: c5Colors.cobaltBlue,
        tooltip: 'Recurring checklist',
      };
    case 'urgent':
      return {
        icon: faExclamationTriangle,
        color: c5Colors.lavaRed,
        tooltip: 'Urgent/Critical',
      };
    case 'one-time':
    default:
      return {
        icon: faClipboardList,
        color: '#757575',
        tooltip: 'One-time checklist',
      };
  }
};

/**
 * Infer template type from checklist name patterns
 * In production, this would come from the backend
 */
const inferTemplateType = (name: string): TemplateType => {
  const lowerName = name.toLowerCase();

  // Recurring patterns
  if (
    lowerName.includes('daily') ||
    lowerName.includes('readiness') ||
    lowerName.includes('shift') ||
    lowerName.includes('hourly') ||
    /\d+am|\d+pm/i.test(name) // Contains time like "2pm"
  ) {
    return 'recurring';
  }

  // Urgent patterns
  if (
    lowerName.includes('urgent') ||
    lowerName.includes('critical') ||
    lowerName.includes('emergency') ||
    lowerName.includes('immediate')
  ) {
    return 'urgent';
  }

  return 'one-time';
};

/**
 * ChecklistCard Component
 */
export const ChecklistCard: React.FC<ChecklistCardProps> = ({
  checklist,
  templateType,
}) => {
  const navigate = useNavigate();

  // Infer template type if not provided
  const inferredType = templateType || inferTemplateType(checklist.name);
  const indicator = getTemplateIndicator(inferredType);

  // Parse assigned positions
  const positions = checklist.assignedPositions
    ? checklist.assignedPositions.split(',').map((p) => p.trim())
    : [];

  // Show first 3 positions, then "+X" for overflow
  const visiblePositions = positions.slice(0, 3);
  const overflowCount = positions.length - 3;

  const handleClick = () => {
    navigate(`/checklists/${checklist.id}`);
  };

  return (
    <Card
      sx={{
        cursor: 'pointer',
        height: 280, // Fixed height for consistency
        display: 'flex',
        flexDirection: 'column',
        transition: 'box-shadow 0.3s',
        '&:hover': {
          boxShadow: 6,
        },
      }}
      onClick={handleClick}
    >
      <CardContent
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          p: 2,
          '&:last-child': {
            pb: 2, // Override MUI default padding
          },
        }}
      >
        {/* Title with template type indicator */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
          <FontAwesomeIcon
            icon={indicator.icon}
            style={{
              color: indicator.color,
              fontSize: '1rem',
              marginRight: 8,
              marginTop: 4,
              flexShrink: 0,
            }}
            title={indicator.tooltip}
          />
          <Typography
            variant="h6"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 2, // Max 2 lines
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              minHeight: '3rem', // Reserve space even if 1 line
              fontSize: '1rem',
              fontWeight: 600,
              lineHeight: 1.5,
            }}
          >
            {checklist.name}
          </Typography>
        </Box>

        {/* Event name - 1 line with ellipsis */}
        <Typography
          variant="body2"
          color="text.secondary"
          noWrap
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            mb: 0.5,
          }}
        >
          {checklist.eventName}
        </Typography>

        {/* Operational period - 1 line with ellipsis */}
        {checklist.operationalPeriodName && (
          <Typography
            variant="caption"
            color="text.secondary"
            noWrap
            sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              mb: 1,
            }}
          >
            {checklist.operationalPeriodName}
          </Typography>
        )}

        {/* Spacer to push progress to bottom */}
        <Box sx={{ flexGrow: 1 }} />

        {/* Progress bar - always at consistent position */}
        <Box sx={{ mt: 2 }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              mb: 0.5,
            }}
          >
            <Typography variant="caption" color="text.secondary">
              Progress
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {checklist.completedItems} / {checklist.totalItems} items
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={Number(checklist.progressPercentage)}
            sx={{
              height: 8,
              borderRadius: 4,
              backgroundColor: '#E0E0E0',
              '& .MuiLinearProgress-bar': {
                backgroundColor: getProgressColor(
                  Number(checklist.progressPercentage)
                ),
              },
            }}
          />
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ display: 'block', mt: 0.5 }}
          >
            {Number(checklist.progressPercentage).toFixed(0)}% complete
          </Typography>
        </Box>

        {/* Assigned positions - max 2 rows with overflow */}
        {positions.length > 0 && (
          <Box
            sx={{
              mt: 1.5,
              display: 'flex',
              flexWrap: 'wrap',
              gap: 0.5,
              maxHeight: '3rem', // Limit to ~2 rows
              overflow: 'hidden',
            }}
          >
            {visiblePositions.map((position, idx) => (
              <Chip
                key={idx}
                label={position}
                size="small"
                sx={{
                  fontSize: '0.7rem',
                  height: 20,
                }}
              />
            ))}
            {overflowCount > 0 && (
              <Chip
                label={`+${overflowCount}`}
                size="small"
                sx={{
                  fontSize: '0.7rem',
                  height: 20,
                  backgroundColor: c5Colors.whiteBlue,
                }}
              />
            )}
          </Box>
        )}

        {/* Created by - always at bottom */}
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ mt: 1 }}
          noWrap
        >
          Created by {checklist.createdBy}
        </Typography>
      </CardContent>
    </Card>
  );
};
