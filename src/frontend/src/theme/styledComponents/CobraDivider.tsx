import { Divider } from '@mui/material';
import { styled } from '@mui/material/styles';

/**
 * CobraDivider - Styled horizontal divider with consistent spacing
 *
 * Use for:
 * - Separating sections in forms
 * - Visual breaks in content
 * - Grouping related fields
 *
 * Example:
 * ```tsx
 * <Stack spacing={2}>
 *   <TextField label="Name" />
 *   <CobraDivider />
 *   <TextField label="Description" />
 * </Stack>
 * ```
 */
export const CobraDivider = styled(Divider)(({ theme }) => ({
  borderColor: theme.palette.divider,
  margin: `${theme.spacing(2)} 0`,
}));
