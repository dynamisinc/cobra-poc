import { Checkbox, CheckboxProps, FormControlLabel } from '@mui/material';
import { FC } from 'react';
import { useTheme } from '@mui/material/styles';

interface ICobraCheckboxProps extends CheckboxProps {
  /**
   * Label text to display next to the checkbox
   */
  label: string;
}

/**
 * CobraCheckbox - Styled checkbox with label
 *
 * Use for:
 * - Boolean form fields
 * - Multi-select options
 * - Task completion toggles
 *
 * Features:
 * - Integrated label (no need for separate FormControlLabel)
 * - Theme-based colors
 * - Accessible
 *
 * Example:
 * ```tsx
 * <CobraCheckbox
 *   label="Mark as complete"
 *   checked={isComplete}
 *   onChange={(e) => setIsComplete(e.target.checked)}
 * />
 * ```
 */
export const CobraCheckbox: FC<ICobraCheckboxProps> = ({ label, ...checkboxProps }) => {
  const theme = useTheme();

  return (
    <FormControlLabel control={
      <Checkbox
        sx={{
          color: theme.palette.primary.contrastText,
          '&.Mui-checked': {
            color: theme.palette.buttonPrimary.main,
          },
          marginLeft:0,
          marginRight:0
        }}
        disableRipple
        inputProps={{ 'aria-label': label }}
        {...checkboxProps}
      />
    }
    label={label} />
  );
};
