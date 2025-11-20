/**
 * CreateChecklistDialog Component
 *
 * Multi-purpose dialog for creating checklists:
 * - From template (fresh instance)
 * - From existing checklist - clean copy (reset status)
 * - From existing checklist - direct copy (preserve status)
 *
 * Smart defaults minimize user friction:
 * - Auto-populates event/period from current context when available
 * - Pre-fills name with sensible defaults
 * - Hides advanced options (positions) unless needed
 *
 * User Stories:
 * - 2.1: Create Checklist from Template
 * - 2.6: Clone Checklist (both modes)
 */

import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Typography,
  Box,
  Alert,
  Collapse,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus, faChevronDown, faChevronUp } from '@fortawesome/free-solid-svg-icons';

/**
 * Mode of checklist creation
 */
export type ChecklistCreationMode = 'from-template' | 'clone-clean' | 'clone-direct';

/**
 * Props for CreateChecklistDialog
 */
interface CreateChecklistDialogProps {
  open: boolean;
  mode: ChecklistCreationMode;

  // For template mode
  templateId?: string;
  templateName?: string;

  // For clone modes
  sourceChecklistId?: string;
  sourceChecklistName?: string;

  // Default values (smart defaults)
  defaultEventId?: string;
  defaultEventName?: string;
  defaultOperationalPeriodId?: string;
  defaultOperationalPeriodName?: string;

  onSave: (data: ChecklistCreationData) => Promise<void>;
  onCancel: () => void;
  saving?: boolean;
}

/**
 * Data for checklist creation
 */
export interface ChecklistCreationData {
  mode: ChecklistCreationMode;
  name: string;
  eventId: string;
  eventName: string;
  operationalPeriodId?: string;
  operationalPeriodName?: string;
  assignedPositions?: string;

  // For template mode
  templateId?: string;

  // For clone modes
  sourceChecklistId?: string;
  preserveStatus?: boolean; // Only for clone modes
}

/**
 * CreateChecklistDialog Component
 */
export const CreateChecklistDialog: React.FC<CreateChecklistDialogProps> = ({
  open,
  mode,
  templateId,
  templateName,
  sourceChecklistId,
  sourceChecklistName,
  defaultEventId = '',
  defaultEventName = '',
  defaultOperationalPeriodId,
  defaultOperationalPeriodName,
  onSave,
  onCancel,
  saving = false,
}) => {
  // Form state
  const [name, setName] = useState('');
  const [eventId, setEventId] = useState(defaultEventId);
  const [eventName, setEventName] = useState(defaultEventName);
  const [operationalPeriodId, setOperationalPeriodId] = useState(defaultOperationalPeriodId || '');
  const [operationalPeriodName, setOperationalPeriodName] = useState(defaultOperationalPeriodName || '');
  const [assignedPositions, setAssignedPositions] = useState('');
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Initialize form when dialog opens
  useEffect(() => {
    if (open) {
      // Reset form
      setError(null);

      // Set default name
      if (mode === 'from-template') {
        setName(templateName ? `${templateName} - ${new Date().toLocaleDateString()}` : '');
      } else if (mode === 'clone-clean' || mode === 'clone-direct') {
        setName(sourceChecklistName ? `${sourceChecklistName} (Copy)` : '');
      }

      // Set event defaults
      setEventId(defaultEventId);
      setEventName(defaultEventName);
      setOperationalPeriodId(defaultOperationalPeriodId || '');
      setOperationalPeriodName(defaultOperationalPeriodName || '');
      setAssignedPositions('');
      setShowAdvanced(false);
    }
  }, [open, mode, templateName, sourceChecklistName, defaultEventId, defaultEventName, defaultOperationalPeriodId, defaultOperationalPeriodName]);

  // Handle save
  const handleSave = async () => {
    // Validation
    if (!name.trim()) {
      setError('Checklist name is required');
      return;
    }

    if (!eventId.trim()) {
      setError('Event ID is required');
      return;
    }

    if (!eventName.trim()) {
      setError('Event name is required');
      return;
    }

    if (mode === 'from-template' && !templateId) {
      setError('Template ID is missing');
      return;
    }

    if ((mode === 'clone-clean' || mode === 'clone-direct') && !sourceChecklistId) {
      setError('Source checklist ID is missing');
      return;
    }

    // Build data object
    const data: ChecklistCreationData = {
      mode,
      name: name.trim(),
      eventId: eventId.trim(),
      eventName: eventName.trim(),
      operationalPeriodId: operationalPeriodId.trim() || undefined,
      operationalPeriodName: operationalPeriodName.trim() || undefined,
      assignedPositions: assignedPositions.trim() || undefined,
    };

    if (mode === 'from-template') {
      data.templateId = templateId;
    } else {
      data.sourceChecklistId = sourceChecklistId;
      data.preserveStatus = mode === 'clone-direct';
    }

    try {
      await onSave(data);
      // Dialog will close via parent state change
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create checklist');
    }
  };

  // Handle cancel
  const handleCancel = () => {
    setError(null);
    onCancel();
  };

  // Get dialog title based on mode
  const getDialogTitle = () => {
    switch (mode) {
      case 'from-template':
        return 'Create Checklist from Template';
      case 'clone-clean':
        return 'Copy Checklist (Clean Copy)';
      case 'clone-direct':
        return 'Copy Checklist (Direct Copy)';
      default:
        return 'Create Checklist';
    }
  };

  // Get dialog description based on mode
  const getDialogDescription = () => {
    switch (mode) {
      case 'from-template':
        return `Creating a new checklist from template: "${templateName}"`;
      case 'clone-clean':
        return `Creating a clean copy (all items reset to not completed) from: "${sourceChecklistName}"`;
      case 'clone-direct':
        return `Creating a direct copy (preserves all completion status and notes) from: "${sourceChecklistName}"`;
      default:
        return '';
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleCancel}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: {
          minHeight: 400,
        },
      }}
    >
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FontAwesomeIcon icon={faPlus} style={{ fontSize: '1.25rem' }} />
          <Typography variant="h6">{getDialogTitle()}</Typography>
        </Box>
      </DialogTitle>

      <DialogContent>
        {/* Description */}
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{
            mb: 3,
            p: 1.5,
            backgroundColor: '#F5F5F5',
            borderRadius: 1,
          }}
        >
          {getDialogDescription()}
        </Typography>

        {/* Error message */}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Name field */}
        <TextField
          fullWidth
          label="Checklist Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          disabled={saving}
          required
          autoFocus
          sx={{ mb: 2 }}
          helperText="Give this checklist a unique name"
        />

        {/* Event ID */}
        <TextField
          fullWidth
          label="Event ID"
          value={eventId}
          onChange={(e) => setEventId(e.target.value)}
          disabled={saving}
          required
          sx={{ mb: 2 }}
          helperText="Example: Hurricane-Milton-2024"
        />

        {/* Event Name */}
        <TextField
          fullWidth
          label="Event Name"
          value={eventName}
          onChange={(e) => setEventName(e.target.value)}
          disabled={saving}
          required
          sx={{ mb: 2 }}
          helperText="Example: Hurricane Milton Response"
        />

        {/* Operational Period (Optional) */}
        <TextField
          fullWidth
          label="Operational Period Name (Optional)"
          value={operationalPeriodName}
          onChange={(e) => setOperationalPeriodName(e.target.value)}
          disabled={saving}
          sx={{ mb: 2 }}
          helperText="Leave blank for incident-level checklist"
        />

        {/* Advanced Options */}
        <Box sx={{ mt: 2 }}>
          <Button
            variant="text"
            size="small"
            onClick={() => setShowAdvanced(!showAdvanced)}
            endIcon={<FontAwesomeIcon icon={showAdvanced ? faChevronUp : faChevronDown} />}
            sx={{ mb: 1 }}
          >
            Advanced Options
          </Button>

          <Collapse in={showAdvanced}>
            <Box sx={{ pl: 2, pr: 2 }}>
              <TextField
                fullWidth
                label="Assigned Positions (Optional)"
                value={assignedPositions}
                onChange={(e) => setAssignedPositions(e.target.value)}
                disabled={saving}
                sx={{ mb: 2 }}
                helperText="Comma-separated list: Incident Commander, Safety Officer"
                placeholder="Leave blank to make visible to all positions"
              />
            </Box>
          </Collapse>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button
          variant="text"
          onClick={handleCancel}
          disabled={saving}
          sx={{
            minWidth: 100,
            minHeight: 48, // C5 minimum touch target
          }}
        >
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving}
          sx={{
            minWidth: 100,
            minHeight: 48, // C5 minimum touch target
          }}
        >
          {saving ? 'Creating...' : 'Create Checklist'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
