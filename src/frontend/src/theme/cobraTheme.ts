import { alpha, createTheme } from "@mui/material/styles";

/**
 * COBRA C5 Design System - Material-UI Theme
 *
 * Standardized theme configuration from cobra-styling-package
 * https://github.com/dynamisinc/cobra-styling-package
 *
 * Key Design Principles:
 * - Silver/Cobalt Blue color scheme
 * - Small form controls by default
 * - 54px header height
 * - Consistent spacing and padding via CobraStyles
 * - Touch-friendly button sizes (48px minimum)
 */

// Extend MUI Theme interface with custom properties
declare module "@mui/material/styles" {
  interface Theme {
    cssStyling: {
      headerHeight: number;
      drawerClosedWidth: number;
      drawerOpenWidth: number;
    };
  }
  interface ThemeOptions {
    cssStyling?: {
      headerHeight: number;
      drawerClosedWidth: number;
      drawerOpenWidth: number;
    };
  }
  interface Palette {
    breadcrumb: {
      background: string;
    };
    border: {
      main: string;
    };
    buttonDelete: {
      contrastText: string;
      dark: string;
      light: string;
      main: string;
    };
    buttonPrimary: {
      contrastText: string;
      dark: string;
      light: string;
      main: string;
    };
    colorPicker: {
      fill: string;
      text: string;
    }
    eventAction: {
      create: string;
      default: string;
      delete: string;
      edit: string;
    };
    grid: {
      light: string;
      main: string;
    };
    linkButton: {
      contrastText: string;
      dark: string;
      light: string;
      main?: string;
    };
    notifications: {
      error: string;
      errorText: string;
      info: string;
      success: string;
      successText: string;
      warning: string;
      warningText: string;
    }
    statusChart: {
      grey: string;
      red: string;
      yellow: string;
      green: string;
      black: string;
    }
  }

  interface PaletteOptions {
    breadcrumb?: {
      background: string;
    };
    border?: {
      main: string;
    };
    buttonDelete?: {
      contrastText: string;
      dark: string;
      light: string;
      main?: string;
    };
    buttonPrimary?: {
      contrastText: string;
      dark: string;
      light: string;
      main?: string;
    };
    colorPicker?: {
      fill: string;
      text: string;
    }
    eventAction?: {
      create: string;
      default: string;
      delete: string;
      edit: string;
    };
    grid: {
      light: string;
      main: string;
    };
    linkButton?: {
      contrastText: string;
      dark: string;
      light: string;
      main?: string;
    };
    notifications?: {
      error: string;
      errorText: string;
      info: string;
      success: string;
      successText: string;
      warning: string;
      warningText: string;
    }
    statusChart?: {
      grey: string;
      red: string;
      yellow: string;
      green: string;
      black: string;
    }
  }
}

// Color overrides for buttons
declare module "@mui/material/Button" {
  interface ButtonPropsColorOverrides {
    white: true;
  }
}

const primaryContrastText = "#1a1a1a";

export const cobraTheme = createTheme({
    cssStyling: {
      drawerClosedWidth: 64,
      drawerOpenWidth: 288,
      headerHeight: 54,
    },
    palette: {
      mode: 'light',
      primary: {
        dark: '#696969', //dim gray
        main: '#c0c0c0', //silver
        contrastText: primaryContrastText,
        light: '#dadbdd', //silver white
      },
      secondary: {
        main: '#b22222', //firebrick
      },
      background: {
        default: '#f8f8f8',
        paper: '#ffffff', //white
      },
      border: {
        main: alpha(primaryContrastText, 0.23)
      },
      breadcrumb: {
        background: '#F1F1F1'
      },
      colorPicker: {
        fill: '#cccccc',
        text: '#333333'
      },
      eventAction: {
        create: '#4caf50', //green
        edit: '#2196f3',   //blue
        delete: '#f44336', //red
        default: '#9e9e9e',//grey
      },
      grid: {
        light: '#EAF2FB',
        main: '#DBE9FA'
      },
      text: {
        primary: '#1a1a1a',
        secondary: '#848482'
      },
      error: {
        main: '#e42217', //lava red
      },
      info: {
        main: '#0020c2',
        light: '#0000ff',
        dark: '#00008b',
      },
      divider: "#848482",
      success: {
        main: '#08682a',
      },
      buttonDelete: {
        contrastText: '#ffffff', //white
        dark: '#c11b17', //chilli pepper
        light: '#ff0000', //red
        main: '#e42217' //lava red
      },
      buttonPrimary: {
        contrastText: '#ffffff', //white
        dark: '#00008b', //darkblue
        light: '#0000ff', //blue
        main: '#0020c2' //cobalt blue
      },
      linkButton: {
        contrastText: '#ffffff', //white
        dark: '#00008b', //darkblue
        light: '#DBE9FA',
        main: '#0020c2' //cobalt blue
      },
      notifications: {
        error: '#EFB6B6',
        errorText: '#b22222',//firebrick
        info: '#DBE9FA',
        success: '#AEFBB8',
        successText: '#008000',//Green
        warning: '#F9F9BE',
        warningText: '#6F4E37',//Logbook brown
      },
      statusChart: {
        grey: '#C0C0C0',
        red: '#C11B17',//chilli pepper
        yellow: '#FFEF00',//canary yellow
        green: '#008000',
        black: '#000000',
      }
    },
    typography: {
      fontFamily: 'Roboto, Arial, sans-serif',
      fontSize: 14,
      button: {
        textTransform: 'none', // Don't force uppercase
      }
    },
    components: {
      MuiIconButton: {
        styleOverrides: {
          root: {
            color: "#1a1a1a",
          }
        }
      },
      // All form controls default to small size
      MuiTextField: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiAutocomplete: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiSelect: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiInputLabel: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiButtonGroup: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiToggleButtonGroup: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiButton: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiListItemIcon: {
        styleOverrides: {
          root: {
            color: '#3A3B3C'
          }
        }
      }
    },
  });

/**
 * Helper function to get progress bar color based on completion percentage
 * 0-33% = red, 34-66% = yellow, 67-99% = cobalt blue, 100% = green
 */
export const getProgressColor = (percentage: number): string => {
  if (percentage === 100) return cobraTheme.palette.success.main;
  if (percentage >= 67) return cobraTheme.palette.info.main;
  if (percentage >= 34) return cobraTheme.palette.statusChart.yellow;
  return cobraTheme.palette.error.main;
};

/**
 * Helper function to get status chip color
 */
export const getStatusChipColor = (status: string): { bg: string; text: string } => {
  const statusLower = status.toLowerCase();

  switch (statusLower) {
    case 'completed':
    case 'complete':
      return { bg: cobraTheme.palette.notifications.success, text: cobraTheme.palette.text.primary };
    case 'in progress':
    case 'in-progress':
      return { bg: cobraTheme.palette.grid.main, text: cobraTheme.palette.text.primary };
    case 'not started':
    case 'not-started':
      return { bg: cobraTheme.palette.primary.light, text: cobraTheme.palette.text.secondary };
    case 'blocked':
      return { bg: cobraTheme.palette.error.main, text: '#ffffff' };
    case 'n/a':
    case 'na':
      return { bg: cobraTheme.palette.primary.main, text: cobraTheme.palette.text.primary };
    default:
      return { bg: cobraTheme.palette.primary.main, text: cobraTheme.palette.text.primary };
  }
};

/**
 * Helper function to get priority chip color
 */
export const getPriorityChipColor = (priority: string): { bg: string; text: string } => {
  const priorityLower = priority.toLowerCase();

  switch (priorityLower) {
    case 'critical':
      return { bg: cobraTheme.palette.error.main, text: '#ffffff' };
    case 'high':
      return { bg: cobraTheme.palette.statusChart.yellow, text: '#000000' };
    case 'medium':
      return { bg: cobraTheme.palette.grid.main, text: cobraTheme.palette.text.primary };
    case 'low':
      return { bg: cobraTheme.palette.primary.light, text: cobraTheme.palette.text.secondary };
    default:
      return { bg: cobraTheme.palette.primary.main, text: cobraTheme.palette.text.primary };
  }
};

export default cobraTheme;
