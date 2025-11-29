/**
 * CreateEventDialog Component
 *
 * Dialog for creating a new event (incident or planned operation).
 * Uses FEMA/NIMS standard event categories.
 *
 * Features:
 * - Event type selection (Planned/Unplanned)
 * - Category selection driven by event type
 * - Optional additional categories
 */

import React, { useState, useEffect } from 'react';
import {
  Box,
  Stack,
  DialogActions,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Typography,
  MenuItem,
  Select,
  InputLabel,
  Chip,
  Autocomplete,
  CircularProgress,
} from '@mui/material';
import {
  CobraDialog,
  CobraTextField,
  CobraSaveButton,
  CobraLinkButton,
} from '../theme/styledComponents';
import CobraStyles from '../theme/CobraStyles';
import { useEvents } from '../hooks/useEvents';
import { eventCategoryService } from '../services/eventService';
import type { EventType, EventCategory, CreateEventRequest } from '../types';

interface CreateEventDialogProps {
  open: boolean;
  onClose: () => void;
  onEventCreated?: (eventId: string) => void;
}

/**
 * CreateEventDialog Component
 */
export const CreateEventDialog: React.FC<CreateEventDialogProps> = ({
  open,
  onClose,
  onEventCreated,
}) => {
  const { createEvent, selectEvent } = useEvents();

  // Form state
  const [name, setName] = useState('');
  const [eventType, setEventType] = useState<EventType>('UNPLANNED');
  const [primaryCategoryId, setPrimaryCategoryId] = useState('');
  const [additionalCategoryIds, setAdditionalCategoryIds] = useState<string[]>([]);

  // Categories state
  const [categories, setCategories] = useState<EventCategory[]>([]);
  const [loadingCategories, setLoadingCategories] = useState(false);

  // UI state
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load categories when event type changes
  useEffect(() => {
    const loadCategories = async () => {
      setLoadingCategories(true);
      try {
        const data = await eventCategoryService.getCategories(eventType);
        setCategories(data);
        // Reset selections when type changes
        setPrimaryCategoryId('');
        setAdditionalCategoryIds([]);
      } catch (err) {
        console.error('Error loading categories:', err);
      } finally {
        setLoadingCategories(false);
      }
    };

    if (open) {
      loadCategories();
    }
  }, [eventType, open]);

  // Reset form when dialog opens
  useEffect(() => {
    if (open) {
      setName('');
      setEventType('UNPLANNED');
      setPrimaryCategoryId('');
      setAdditionalCategoryIds([]);
      setError(null);
    }
  }, [open]);

  // Group categories by SubGroup for display
  const categoriesByGroup = categories.reduce((acc, cat) => {
    if (!acc[cat.subGroup]) {
      acc[cat.subGroup] = [];
    }
    acc[cat.subGroup].push(cat);
    return acc;
  }, {} as Record<string, EventCategory[]>);

  const handleEventTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEventType(e.target.value as EventType);
  };

  const handleSubmit = async () => {
    // Validation
    if (!name.trim()) {
      setError('Event name is required');
      return;
    }
    if (!primaryCategoryId) {
      setError('Primary category is required');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      const request: CreateEventRequest = {
        name: name.trim(),
        eventType,
        primaryCategoryId,
        additionalCategoryIds: additionalCategoryIds.length > 0 ? additionalCategoryIds : undefined,
      };

      const newEvent = await createEvent(request);

      // Select the new event as current
      selectEvent(newEvent);

      if (onEventCreated) {
        onEventCreated(newEvent.id);
      }

      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to create event';
      setError(message);
    } finally {
      setSaving(false);
    }
  };

  // Get primary category display name
  const primaryCategory = categories.find(c => c.id === primaryCategoryId);

  // Available additional categories (exclude primary)
  const additionalOptions = categories.filter(c => c.id !== primaryCategoryId);

  return (
    <CobraDialog
      open={open}
      onClose={onClose}
      title="Create New Event"
      contentWidth="500px"
    >
      <Stack spacing={CobraStyles.Spacing.FormFields}>
        {/* Event Name */}
        <CobraTextField
          label="Event Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          fullWidth
          required
          placeholder="e.g., Hurricane Milton Response"
          error={!!error && !name.trim()}
          helperText={error && !name.trim() ? 'Event name is required' : ''}
        />

        {/* Event Type */}
        <FormControl component="fieldset">
          <FormLabel
            component="legend"
            sx={{ fontSize: '0.875rem', fontWeight: 'bold', mb: 1 }}
          >
            Event Type *
          </FormLabel>
          <RadioGroup
            row
            value={eventType}
            onChange={handleEventTypeChange}
          >
            <FormControlLabel
              value="UNPLANNED"
              control={<Radio size="small" />}
              label={
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                    Unplanned
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Emergency incidents
                  </Typography>
                </Box>
              }
              sx={{ mr: 4 }}
            />
            <FormControlLabel
              value="PLANNED"
              control={<Radio size="small" />}
              label={
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                    Planned
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Scheduled events
                  </Typography>
                </Box>
              }
            />
          </RadioGroup>
        </FormControl>

        {/* Primary Category */}
        <FormControl fullWidth required error={!!error && !primaryCategoryId}>
          <InputLabel id="primary-category-label">Primary Category</InputLabel>
          <Select
            labelId="primary-category-label"
            value={primaryCategoryId}
            label="Primary Category"
            onChange={(e) => setPrimaryCategoryId(e.target.value)}
            disabled={loadingCategories}
            sx={{ backgroundColor: 'white' }}
          >
            {loadingCategories && (
              <MenuItem disabled>
                <CircularProgress size={16} sx={{ mr: 1 }} />
                Loading categories...
              </MenuItem>
            )}
            {Object.entries(categoriesByGroup).map(([group, cats]) => [
              <MenuItem key={`header-${group}`} disabled sx={{ fontWeight: 'bold', opacity: 1 }}>
                {group}
              </MenuItem>,
              ...cats.map((cat) => (
                <MenuItem key={cat.id} value={cat.id} sx={{ pl: 3 }}>
                  {cat.name}
                </MenuItem>
              )),
            ])}
          </Select>
          {error && !primaryCategoryId && (
            <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
              Primary category is required
            </Typography>
          )}
        </FormControl>

        {/* Additional Categories */}
        <Autocomplete
          multiple
          options={additionalOptions}
          getOptionLabel={(option) => option.name}
          value={categories.filter(c => additionalCategoryIds.includes(c.id))}
          onChange={(_, newValue) => {
            setAdditionalCategoryIds(newValue.map(v => v.id));
          }}
          groupBy={(option) => option.subGroup}
          disabled={loadingCategories || !primaryCategoryId}
          renderInput={(params) => (
            <CobraTextField
              {...params}
              label="Additional Categories (Optional)"
              placeholder="Select additional categories..."
            />
          )}
          renderTags={(value, getTagProps) =>
            value.map((option, index) => (
              <Chip
                label={option.name}
                size="small"
                {...getTagProps({ index })}
                key={option.id}
              />
            ))
          }
        />

        {/* Error message */}
        {error && name.trim() && primaryCategoryId && (
          <Typography variant="body2" color="error">
            {error}
          </Typography>
        )}

        {/* Preview */}
        {name.trim() && primaryCategory && (
          <Box
            sx={{
              p: 1.5,
              backgroundColor: 'rgba(0, 32, 194, 0.05)',
              borderRadius: 1,
              border: '1px solid rgba(0, 32, 194, 0.2)',
            }}
          >
            <Typography variant="caption" color="text.secondary">
              Preview:
            </Typography>
            <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
              {name}
            </Typography>
            <Typography variant="caption">
              {eventType} - {primaryCategory.name}
              {additionalCategoryIds.length > 0 && (
                <> + {additionalCategoryIds.length} more</>
              )}
            </Typography>
          </Box>
        )}

        {/* Actions */}
        <DialogActions sx={{ px: 0, pb: 0 }}>
          <CobraLinkButton onClick={onClose}>Cancel</CobraLinkButton>
          <CobraSaveButton
            onClick={handleSubmit}
            isSaving={saving}
            disabled={!name.trim() || !primaryCategoryId || saving}
          >
            Create Event
          </CobraSaveButton>
        </DialogActions>
      </Stack>
    </CobraDialog>
  );
};
