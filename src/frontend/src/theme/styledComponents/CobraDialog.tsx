import { Dialog, DialogContent, DialogTitle, IconButton } from '@mui/material';
import { PropsWithChildren } from 'react';
import { useTheme } from '@mui/material/styles';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faXmark } from '@fortawesome/free-solid-svg-icons';
import CobraStyles from '../CobraStyles';

type CobraDialogProps = {
  /**
   * Custom height for dialog content
   */
  contentHeight?: string | number;

  /**
   * Custom width for dialog content
   */
  contentWidth?: string | number;

  /**
   * Custom width for the entire dialog
   */
  customDialogWidth?: string | number;

  /**
   * Hide the close (X) button in top-right corner
   */
  hideCloseButton?: boolean;

  /**
   * Callback when dialog should close
   */
  onClose?: () => void;

  /**
   * Whether the dialog is open
   */
  open: boolean;

  /**
   * Dialog title text (appears in silver header bar)
   */
  title?: string;
}

/**
 * CobraDialog - Standardized modal dialog with COBRA styling
 *
 * Use for:
 * - Forms (create/edit)
 * - Confirmations
 * - Detail views
 * - Any modal content
 *
 * Features:
 * - Silver header bar (40px height)
 * - Close button in top-right (unless hideCloseButton is true)
 * - Consistent padding (15px from CobraStyles)
 * - Customizable dimensions
 *
 * Example:
 * ```tsx
 * <CobraDialog
 *   open={isOpen}
 *   onClose={handleClose}
 *   title="Create New Checklist"
 *   contentWidth="600px"
 * >
 *   <Stack spacing={CobraStyles.Spacing.FormFields}>
 *     <CobraTextField label="Name" />
 *     <DialogActions>
 *       <CobraLinkButton onClick={handleClose}>Cancel</CobraLinkButton>
 *       <CobraSaveButton onClick={handleSave}>Save</CobraSaveButton>
 *     </DialogActions>
 *   </Stack>
 * </CobraDialog>
 * ```
 */
export const CobraDialog = ({
  children,
  contentHeight,
  contentWidth,
  hideCloseButton,
  onClose,
  open,
  title,
  customDialogWidth
}: PropsWithChildren<CobraDialogProps>) => {
  const theme = useTheme();

  const DialogStyles = {
    backgroundColor: theme.palette.background.default,
    width: customDialogWidth ? customDialogWidth : "unset"
  };

  const DialogContentStyles = {
    height: contentHeight,
    width: contentWidth,
    padding: CobraStyles.Padding.DialogContent
  };

  return (
    <Dialog
      open={open}
      slotProps={{ paper: { style: DialogStyles } }}
      aria-labelledby="modal-title"
      maxWidth={false}
    >
      {title != null &&
        <DialogTitle
          id="modal-title"
          style={{
            backgroundColor: theme.palette.primary.main,
            height: "40px",
            paddingLeft: CobraStyles.Padding.DialogContent,
            paddingTop: 5
          }}
        >
          {title}
        </DialogTitle>
      }

      {!hideCloseButton && (
        <IconButton
          aria-label="Close"
          onClick={onClose}
          sx={{
            padding: "4px",
            position: 'absolute',
            right: 8,
            top: 2
          }}
        >
          <FontAwesomeIcon icon={faXmark} />
        </IconButton>
      )}

      <DialogContent style={DialogContentStyles}>
        {children}
      </DialogContent>
    </Dialog>
  );
};
