import { ButtonProps } from '@mui/material';
import { CobraPrimaryButton } from './CobraPrimaryButton';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faFloppyDisk, faSpinner } from '@fortawesome/free-solid-svg-icons';

interface SaveButtonProps extends ButtonProps {
  /**
   * Whether the save operation is in progress
   * When true, shows a spinner icon and disables the button
   */
  isSaving?: boolean;
}

/**
 * CobraSaveButton - Save button with floppy disk icon and loading state
 *
 * Use for:
 * - Form save actions
 * - Persisting changes
 * - Update operations
 *
 * Features:
 * - Shows spinner when isSaving is true
 * - Automatically disables during save
 * - Clear visual feedback
 *
 * Example:
 * ```tsx
 * <CobraSaveButton onClick={handleSave} isSaving={loading}>Save Changes</CobraSaveButton>
 * ```
 */
export const CobraSaveButton = (props: SaveButtonProps) => {
  const { isSaving, ...buttonProps } = props;

  return (
    <CobraPrimaryButton
      {...buttonProps}
      startIcon={isSaving === true ?
        <FontAwesomeIcon icon={faSpinner} spin />
        :
        <FontAwesomeIcon icon={faFloppyDisk} />}
      disabled={props.disabled || isSaving === true}
    >
      {props.children}
    </CobraPrimaryButton>
  );
};
