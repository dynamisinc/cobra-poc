import { Button } from '@mui/material';
import { styled } from '@mui/material/styles';

/**
 * CobraSecondaryButton - Secondary action button with white background and blue border
 *
 * Use for:
 * - Secondary actions
 * - Alternative options
 * - Less prominent actions
 *
 * Example:
 * ```tsx
 * <CobraSecondaryButton onClick={handlePreview}>Preview</CobraSecondaryButton>
 * ```
 */
export const CobraSecondaryButton = styled(Button)(({ theme }) => ({
  background: theme.palette.buttonPrimary.contrastText,
  border: 2,
  borderRadius: 50,
  borderStyle: 'solid',
  color:  theme.palette.buttonPrimary.main,
  paddingBottom: 3,
  paddingLeft: 20,
  paddingRight: 20,
  paddingTop: 3,
  textTransform: 'none',
  '&:hover': {
    borderColor: theme.palette.buttonPrimary.light,
    color: theme.palette.buttonPrimary.light
  },
  '&:active': {
    borderColor: theme.palette.buttonPrimary.dark,
    color: theme.palette.buttonPrimary.dark
  },
  "&.Mui-disabled": {
    borderColor: theme.palette.primary.dark,
    color: theme.palette.primary.dark
  }
}));
