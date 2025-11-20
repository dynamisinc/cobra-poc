import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  FormLabel,
  RadioGroup,
  Radio,
  Box,
  Typography,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus, faEdit } from '@fortawesome/free-solid-svg-icons';
import { toast } from 'react-toastify';
import { itemLibraryService } from '../services/itemLibraryService';
import { StatusConfigurationBuilder } from './StatusConfigurationBuilder';
import type { ItemLibraryEntry, CreateItemLibraryEntryRequest, StatusOption } from '../types';
import { ItemType, DEFAULT_STATUS_OPTIONS } from '../types';

interface ItemLibraryItemDialogProps {
  open: boolean;
  onClose: () => void;
  onSaved: () => void;
  /** If provided, dialog is in edit mode */
  existingItem?: ItemLibraryEntry;
}

/**
 * ItemLibraryItemDialog Component
 *
 * Dialog for creating new library items or editing existing ones.
 * Supports both checkbox and status dropdown items with full configuration.
 */
export const ItemLibraryItemDialog: React.FC<ItemLibraryItemDialogProps> = ({
  open,
  onClose,
  onSaved,
  existingItem,
}) => {
  const isEditMode = !!existingItem;

  // Form state
  const [itemText, setItemText] = useState('');
  const [itemType, setItemType] = useState<ItemType>(ItemType.CHECKBOX);
  const [category, setCategory] = useState('');
  const [tags, setTags] = useState('');
  const [isRequiredByDefault, setIsRequiredByDefault] = useState(false);
  const [statusConfiguration, setStatusConfiguration] = useState<StatusOption[]>(DEFAULT_STATUS_OPTIONS);
  const [defaultNotes, setDefaultNotes] = useState('');

  const [saving, setSaving] = useState(false);

  // Pre-fill form when dialog opens (create or edit mode)
  useEffect(() => {
    if (open) {
      if (existingItem) {
        // Edit mode - load existing data
        setItemText(existingItem.itemText);
        setItemType(existingItem.itemType as ItemType);
        setCategory(existingItem.category);
        setTags(existingItem.tags ? JSON.parse(existingItem.tags).join(', ') : '');
        setIsRequiredByDefault(existingItem.isRequiredByDefault);
        setStatusConfiguration(
          existingItem.statusConfiguration
            ? JSON.parse(existingItem.statusConfiguration)
            : DEFAULT_STATUS_OPTIONS
        );
        setDefaultNotes(existingItem.defaultNotes || '');
      } else {
        // Create mode - reset to defaults
        setItemText('');
        setItemType(ItemType.CHECKBOX);
        setCategory('');
        setTags('');
        setIsRequiredByDefault(false);
        setStatusConfiguration(DEFAULT_STATUS_OPTIONS);
        setDefaultNotes('');
      }
    }
  }, [open, existingItem]);

  const handleSave = async () => {
    // Validation
    if (!itemText.trim()) {
      toast.error('Item text is required');
      return;
    }
    if (!category.trim()) {
      toast.error('Category is required');
      return;
    }
    if (itemType === ItemType.STATUS && statusConfiguration.length === 0) {
      toast.error('Status items must have at least one status option');
      return;
    }

    try {
      setSaving(true);

      // Parse tags
      const tagArray = tags
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean);

      const requestData: CreateItemLibraryEntryRequest = {
        itemText: itemText.trim(),
        itemType,
        category: category.trim(),
        statusConfiguration:
          itemType === 'status' && statusConfiguration.length > 0
            ? JSON.stringify(statusConfiguration)
            : undefined,
        allowedPositions: undefined, // Not exposed in this simple dialog
        defaultNotes: defaultNotes.trim() || undefined,
        tags: tagArray.length > 0 ? tagArray : undefined,
        isRequiredByDefault,
      };

      if (isEditMode && existingItem) {
        await itemLibraryService.updateLibraryItem(existingItem.id, requestData);
        toast.success('Library item updated!');
      } else {
        await itemLibraryService.createLibraryItem(requestData);
        toast.success('Library item created!');
      }

      onSaved();
      handleClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save library item';
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  const handleClose = () => {
    if (!saving) {
      onClose();
    }
  };

  // Common categories
  const commonCategories = [
    'Safety',
    'Operations',
    'Planning',
    'Logistics',
    'Finance/Admin',
    'Communications',
    'Medical',
    'Transportation',
    'Equipment',
    'Documentation',
    'Other',
  ];

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FontAwesomeIcon icon={isEditMode ? faEdit : faPlus} />
          <Typography variant="h6" component="span">
            {isEditMode ? 'Edit Library Item' : 'Create Library Item'}
          </Typography>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
          {/* Item Text */}
          <TextField
            fullWidth
            label="Item Text"
            placeholder="e.g., Verify all personnel have safety equipment"
            value={itemText}
            onChange={(e) => setItemText(e.target.value)}
            required
            multiline
            rows={2}
            helperText="Clear, actionable description of the task"
          />

          {/* Item Type */}
          <FormControl component="fieldset" required>
            <FormLabel component="legend">Item Type</FormLabel>
            <RadioGroup
              row
              value={itemType}
              onChange={(e) => setItemType(e.target.value as ItemType)}
            >
              <FormControlLabel value="checkbox" control={<Radio />} label="Checkbox" />
              <FormControlLabel value="status" control={<Radio />} label="Status Dropdown" />
            </RadioGroup>
          </FormControl>

          {/* Status Configuration (only for status items) */}
          {itemType === 'status' && (
            <Box>
              <Typography variant="subtitle2" sx={{ mb: 1 }}>
                Status Options *
              </Typography>
              <StatusConfigurationBuilder
                value={statusConfiguration}
                onChange={setStatusConfiguration}
              />
            </Box>
          )}

          {/* Category */}
          <FormControl fullWidth required>
            <InputLabel>Category</InputLabel>
            <Select value={category} label="Category" onChange={(e) => setCategory(e.target.value)}>
              {commonCategories.map((cat) => (
                <MenuItem key={cat} value={cat}>
                  {cat}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Tags */}
          <TextField
            fullWidth
            label="Tags (comma separated)"
            placeholder="safety, daily, equipment"
            value={tags}
            onChange={(e) => setTags(e.target.value)}
            helperText="Tags help users find this item when searching"
          />

          {/* Default Notes */}
          <TextField
            fullWidth
            label="Default Notes (optional)"
            placeholder="e.g., Check with safety officer before marking complete"
            value={defaultNotes}
            onChange={(e) => setDefaultNotes(e.target.value)}
            multiline
            rows={2}
            helperText="These notes will appear when this item is added to templates"
          />

          {/* Is Required by Default */}
          <FormControlLabel
            control={
              <Checkbox
                checked={isRequiredByDefault}
                onChange={(e) => setIsRequiredByDefault(e.target.checked)}
              />
            }
            label="Mark as 'Required' by default when added to templates"
          />
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={handleClose} variant="text" disabled={saving}>
          Cancel
        </Button>
        <Button onClick={handleSave} variant="contained" disabled={saving}>
          {saving ? 'Saving...' : isEditMode ? 'Save Changes' : 'Create Item'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
