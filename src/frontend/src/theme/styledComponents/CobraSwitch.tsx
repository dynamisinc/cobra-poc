import { Switch, SwitchProps, FormControlLabel } from '@mui/material';
import { FC } from 'react';
import { useTheme } from '@mui/material/styles';

interface ICobraSwitchProps extends SwitchProps {
  /**
   * Label text to display next to the switch
   */
  label: string;
}

/**
 * CobraSwitch - Styled toggle switch with label
 *
 * Use for:
 * - Boolean settings
 * - Feature toggles
 * - On/off states
 *
 * Features:
 * - Integrated label
 * - Cobalt blue when checked
 * - Smooth transitions
 *
 * Example:
 * ```tsx
 * <CobraSwitch
 *   label="Enable notifications"
 *   checked={notificationsEnabled}
 *   onChange={(e) => setNotificationsEnabled(e.target.checked)}
 * />
 * ```
 */
export const CobraSwitch: FC<ICobraSwitchProps> = ({ label, ...switchProps }) => {
  const theme = useTheme();

  return (
    <FormControlLabel
      control={
        <Switch
          sx={{
            '& .MuiSwitch-switchBase.Mui-checked': {
              color: theme.palette.buttonPrimary.main,
            },
            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
              backgroundColor: theme.palette.buttonPrimary.main,
            },
          }}
          {...switchProps}
        />
      }
      label={label}
    />
  );
};
