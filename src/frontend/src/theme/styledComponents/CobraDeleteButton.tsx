import { Button, ButtonProps } from '@mui/material';
import { styled } from '@mui/material/styles';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTrashCan } from '@fortawesome/free-solid-svg-icons';

/**
 * Styled button component for delete actions
 */
const StyledButton = styled(Button)(({ theme }) => ({
  background:  theme.palette.buttonDelete.main,
  borderRadius: 50,
  color: theme.palette.buttonDelete.contrastText,
  paddingBottom: 5,
  paddingLeft: 20,
  paddingRight: 20,
  paddingTop: 5,
  textTransform: 'none',
  '&:hover': {
    background: theme.palette.buttonDelete.light
  },
  '&:active': {
    background: theme.palette.buttonDelete.dark
  }
}));

/**
 * CobraDeleteButton - Delete/remove action button with red background and trash icon
 *
 * Use for:
 * - Delete actions
 * - Remove items
 * - Destructive actions
 *
 * Example:
 * ```tsx
 * <CobraDeleteButton onClick={handleDelete}>Delete Template</CobraDeleteButton>
 * ```
 */
export const CobraDeleteButton = (props: ButtonProps) => {
  return (
    <StyledButton {...props} startIcon={<FontAwesomeIcon icon={faTrashCan} />}>
      {props.children}
    </StyledButton>
  );
};
