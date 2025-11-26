import { Button } from '@mui/material';
import { styled } from '@mui/material/styles';

/**
 * CobraLinkButton - Text-style button for cancel/back/navigation actions
 *
 * Use for:
 * - Cancel actions
 * - Back navigation
 * - Dismiss actions
 * - Secondary navigation
 *
 * Example:
 * ```tsx
 * <CobraLinkButton onClick={handleCancel}>Cancel</CobraLinkButton>
 * ```
 */
export const CobraLinkButton = styled(Button)(({ theme }) => ({
  borderRadius: 50,
  color: theme.palette.linkButton.main,
  cursor: 'pointer',
  paddingBottom: 5,
  paddingLeft: 20,
  paddingRight: 20,
  paddingTop: 5,
  textTransform: 'none',
  textDecoration: 'none',
  '&:hover': {
    backgroundColor: theme.palette.linkButton.light,
  },
  '&:active': {
    color: theme.palette.linkButton.dark
  },
  "&.Mui-disabled": {
    color: theme.palette.primary.light
  }
}));
