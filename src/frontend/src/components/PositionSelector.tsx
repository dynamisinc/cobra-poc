/**
 * PositionSelector Component
 *
 * Multi-select dropdown for changing user positions in POC.
 * Allows testing position-based filtering without database changes.
 *
 * Features:
 * - Multi-select with checkboxes
 * - Dummy ICS positions (no DB required)
 * - Updates mock user context on change
 * - Shows current position(s) in chip format
 */

import React, { useState } from 'react';
import {
  FormControl,
  Select,
  MenuItem,
  Checkbox,
  ListItemText,
  Chip,
  Box,
  SelectChangeEvent,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUserShield } from '@fortawesome/free-solid-svg-icons';
import { setMockUser, getCurrentUser } from '../services/api';
import { c5Colors } from '../theme/c5Theme';

/**
 * Standard ICS positions for emergency management
 * These are dummy values - no database changes required
 */
const ICS_POSITIONS = [
  'Incident Commander',
  'Safety Officer',
  'Public Information Officer',
  'Liaison Officer',
  'Operations Section Chief',
  'Planning Section Chief',
  'Logistics Chief',
  'Finance/Admin Chief',
  'Shelter Manager',
] as const;

interface PositionSelectorProps {
  /**
   * Callback when positions change (for refreshing data)
   */
  onPositionChange?: (positions: string[]) => void;
}

export const PositionSelector: React.FC<PositionSelectorProps> = ({
  onPositionChange,
}) => {
  const currentUser = getCurrentUser();

  // Parse current position(s) - could be single or comma-separated
  const initialPositions = currentUser.position
    .split(',')
    .map((p) => p.trim())
    .filter((p) => p.length > 0);

  const [selectedPositions, setSelectedPositions] = useState<string[]>(
    initialPositions.length > 0 ? initialPositions : ['Incident Commander']
  );

  /**
   * Handle position selection change
   */
  const handleChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value;
    const positions =
      typeof value === 'string' ? value.split(',') : (value as string[]);

    // Ensure at least one position is selected
    if (positions.length === 0) {
      return;
    }

    setSelectedPositions(positions);

    // Update mock user context (use first position as primary)
    setMockUser({
      ...currentUser,
      position: positions[0], // Backend expects single position
    });

    // Notify parent component to refresh data
    if (onPositionChange) {
      onPositionChange(positions);
    }
  };

  /**
   * Handle chip delete (remove position)
   */
  const handleDelete = (positionToRemove: string) => {
    const newPositions = selectedPositions.filter(
      (p) => p !== positionToRemove
    );

    // Keep at least one position
    if (newPositions.length === 0) {
      return;
    }

    setSelectedPositions(newPositions);

    // Update mock user
    setMockUser({
      ...currentUser,
      position: newPositions[0],
    });

    // Notify parent
    if (onPositionChange) {
      onPositionChange(newPositions);
    }
  };

  return (
    <FormControl
      size="small"
      sx={{
        minWidth: 200,
        backgroundColor: 'rgba(255, 255, 255, 0.15)',
        borderRadius: 1,
        '& .MuiOutlinedInput-root': {
          color: 'white',
          '& fieldset': {
            borderColor: 'rgba(255, 255, 255, 0.3)',
          },
          '&:hover fieldset': {
            borderColor: 'rgba(255, 255, 255, 0.5)',
          },
          '&.Mui-focused fieldset': {
            borderColor: 'white',
          },
        },
        '& .MuiSvgIcon-root': {
          color: 'white',
        },
      }}
    >
      <Select
        multiple
        value={selectedPositions}
        onChange={handleChange}
        renderValue={(selected) => (
          <Box sx={{ display: 'flex', gap: 0.5, alignItems: 'center' }}>
            <FontAwesomeIcon
              icon={faUserShield}
              style={{ marginRight: 8 }}
              size="sm"
            />
            {selected.length === 1 ? (
              <span>{selected[0]}</span>
            ) : (
              <span>{selected[0]} +{selected.length - 1}</span>
            )}
          </Box>
        )}
        MenuProps={{
          PaperProps: {
            sx: {
              maxHeight: 400,
              '& .MuiMenuItem-root': {
                '&:hover': {
                  backgroundColor: c5Colors.whiteBlue,
                },
              },
            },
          },
        }}
      >
        {ICS_POSITIONS.map((position) => (
          <MenuItem key={position} value={position}>
            <Checkbox
              checked={selectedPositions.indexOf(position) > -1}
              sx={{
                color: c5Colors.cobaltBlue,
                '&.Mui-checked': {
                  color: c5Colors.cobaltBlue,
                },
              }}
            />
            <ListItemText primary={position} />
          </MenuItem>
        ))}
      </Select>

      {/* Show selected positions as chips below dropdown when open */}
      {selectedPositions.length > 1 && (
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 0.5,
            mt: 1,
            px: 1,
            pb: 1,
          }}
        >
          {selectedPositions.map((position) => (
            <Chip
              key={position}
              label={position}
              size="small"
              onDelete={() => handleDelete(position)}
              sx={{
                backgroundColor: 'rgba(255, 255, 255, 0.9)',
                color: c5Colors.cobaltBlue,
                fontWeight: 500,
                '& .MuiChip-deleteIcon': {
                  color: c5Colors.cobaltBlue,
                  '&:hover': {
                    color: c5Colors.lavaRed,
                  },
                },
              }}
            />
          ))}
        </Box>
      )}
    </FormControl>
  );
};
