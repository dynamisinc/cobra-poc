import { ButtonProps } from '@mui/material';
import { CobraPrimaryButton } from './CobraPrimaryButton';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus } from '@fortawesome/free-solid-svg-icons';

/**
 * CobraNewButton - "Create New" button with plus icon
 *
 * Use for:
 * - Creating new entities (templates, checklists, items)
 * - Add actions
 * - New record creation
 *
 * Example:
 * ```tsx
 * <CobraNewButton onClick={handleCreate}>Create Checklist</CobraNewButton>
 * ```
 */
export const CobraNewButton = (props: ButtonProps) => {
  return (
    <CobraPrimaryButton {...props} startIcon={<FontAwesomeIcon icon={faPlus} />} type='button'>
      {props.children}
    </CobraPrimaryButton>
  );
};
