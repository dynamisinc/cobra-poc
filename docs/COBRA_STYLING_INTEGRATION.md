# COBRA Styling Integration Guide

> **Last Updated:** 2025-11-24
> **Source:** [cobra-styling-package](https://github.com/dynamisinc/cobra-styling-package)

## Overview

This project uses the **COBRA standardized styling system** for consistent design across all COBRA prototypes and applications. The styling package provides:

- Standardized Material-UI theme configuration
- Pre-built styled components (buttons, forms, dialogs)
- Consistent spacing and padding constants
- Silver/Cobalt Blue color palette
- Touch-friendly component sizes

## Quick Start

### Importing Styled Components

**❌ DON'T DO THIS:**
```tsx
import { Button, TextField, Dialog } from '@mui/material';
```

**✅ DO THIS INSTEAD:**
```tsx
import {
  CobraPrimaryButton,
  CobraTextField,
  CobraDialog
} from 'theme/styledComponents';
```

### Using Spacing Constants

**❌ DON'T DO THIS:**
```tsx
<Stack spacing={2} padding="20px">
```

**✅ DO THIS INSTEAD:**
```tsx
import CobraStyles from 'theme/CobraStyles';

<Stack
  spacing={CobraStyles.Spacing.FormFields}
  padding={CobraStyles.Padding.MainWindow}
>
```

### Accessing Theme Colors

**❌ DON'T DO THIS:**
```tsx
<Box sx={{ backgroundColor: '#0020c2' }}>
```

**✅ DO THIS INSTEAD:**
```tsx
import { useTheme } from '@mui/material/styles';

const theme = useTheme();
<Box sx={{ backgroundColor: theme.palette.buttonPrimary.main }}>
```

---

## Available Components

### Buttons

| Component | Use Case | Example |
|-----------|----------|---------|
| **CobraPrimaryButton** | Main actions (Save, Create, Submit) | `<CobraPrimaryButton onClick={handleSave}>Save</CobraPrimaryButton>` |
| **CobraSecondaryButton** | Alternative actions | `<CobraSecondaryButton onClick={handlePreview}>Preview</CobraSecondaryButton>` |
| **CobraDeleteButton** | Delete/remove actions | `<CobraDeleteButton onClick={handleDelete}>Delete</CobraDeleteButton>` |
| **CobraNewButton** | Create new entity | `<CobraNewButton onClick={handleCreate}>Create Checklist</CobraNewButton>` |
| **CobraSaveButton** | Save with loading state | `<CobraSaveButton isSaving={loading}>Save Changes</CobraSaveButton>` |
| **CobraLinkButton** | Cancel, back, dismiss | `<CobraLinkButton onClick={handleCancel}>Cancel</CobraLinkButton>` |

### Form Controls

| Component | Use Case | Example |
|-----------|----------|---------|
| **CobraTextField** | Text inputs | `<CobraTextField label="Name" value={name} onChange={...} fullWidth />` |
| **CobraCheckbox** | Boolean inputs | `<CobraCheckbox label="Mark complete" checked={done} onChange={...} />` |
| **CobraSwitch** | Toggle switches | `<CobraSwitch label="Enable notifications" checked={enabled} onChange={...} />` |

### Layout Components

| Component | Use Case | Example |
|-----------|----------|---------|
| **CobraDialog** | Modal dialogs | See [Dialog Example](#dialog-example) below |
| **CobraDivider** | Section separators | `<CobraDivider />` |

---

## Spacing & Padding Constants

### CobraStyles.Padding

| Constant | Value | Use Case |
|----------|-------|----------|
| `MainWindow` | 18px | Page content padding |
| `DialogContent` | 15px | Dialog interior padding |
| `PopoverContent` | 12px | Popover/tooltip padding |

### CobraStyles.Spacing

| Constant | Value | Use Case |
|----------|-------|----------|
| `AfterSeparator` | 18px | Spacing after dividers |
| `DashboardWidgets` | 20px | Dashboard widget gaps |
| `IconAndText` | 5px | Icon-to-text spacing |
| `FormFields` | 12px | Spacing between form fields |

**Example Usage:**
```tsx
import CobraStyles from 'theme/CobraStyles';
import { Stack } from '@mui/material';

<Stack
  direction="column"
  spacing={CobraStyles.Spacing.FormFields}
  padding={CobraStyles.Padding.MainWindow}
>
  <CobraTextField label="Field 1" />
  <CobraTextField label="Field 2" />
</Stack>
```

---

## Color Palette

### Primary Colors

| Color | Hex | Theme Path | Use Case |
|-------|-----|------------|----------|
| **Dim Gray** | #696969 | `theme.palette.primary.dark` | Dark text, icons |
| **Silver** | #c0c0c0 | `theme.palette.primary.main` | Dialog headers |
| **Silver White** | #dadbdd | `theme.palette.primary.light` | Disabled states |

### Action Colors

| Color | Hex | Theme Path | Use Case |
|-------|-----|------------|----------|
| **Cobalt Blue** | #0020c2 | `theme.palette.buttonPrimary.main` | Primary buttons |
| **Blue** | #0000ff | `theme.palette.buttonPrimary.light` | Button hover |
| **Dark Blue** | #00008b | `theme.palette.buttonPrimary.dark` | Button active |
| **Lava Red** | #e42217 | `theme.palette.buttonDelete.main` | Delete buttons |
| **Red** | #ff0000 | `theme.palette.buttonDelete.light` | Delete hover |

### Background Colors

| Color | Hex | Theme Path | Use Case |
|-------|-----|------------|----------|
| **Light Gray** | #f8f8f8 | `theme.palette.background.default` | Page background |
| **White** | #ffffff | `theme.palette.background.paper` | Card background |

### Status Colors

| Color | Hex | Theme Path | Use Case |
|-------|-----|------------|----------|
| **Dark Green** | #08682a | `theme.palette.success.main` | Success messages |
| **Success Green** | #AEFBB8 | `theme.palette.notifications.success` | Success toast |
| **Canary Yellow** | #FFEF00 | `theme.palette.statusChart.yellow` | Warning states |

### Grid Selection Colors

| Color | Hex | Theme Path | Use Case |
|-------|-----|------------|----------|
| **White Blue** | #DBE9FA | `theme.palette.grid.main` | Selected rows |
| **Light Blue** | #EAF2FB | `theme.palette.grid.light` | Child/detail rows |

---

## Common Patterns

### Form Layout Pattern

```tsx
import { Stack, DialogActions } from '@mui/material';
import {
  CobraTextField,
  CobraSaveButton,
  CobraLinkButton,
  CobraDivider
} from 'theme/styledComponents';
import CobraStyles from 'theme/CobraStyles';

<Stack spacing={CobraStyles.Spacing.FormFields}>
  {/* Form fields */}
  <CobraTextField label="Name" fullWidth required />
  <CobraTextField label="Description" fullWidth multiline rows={4} />

  <CobraDivider />

  {/* Action buttons */}
  <DialogActions>
    <CobraLinkButton onClick={handleCancel}>Cancel</CobraLinkButton>
    <CobraSaveButton onClick={handleSave} isSaving={loading}>
      Save Changes
    </CobraSaveButton>
  </DialogActions>
</Stack>
```

### Page Layout Pattern

```tsx
import { Stack, Typography } from '@mui/material';
import { CobraNewButton } from 'theme/styledComponents';
import CobraStyles from 'theme/CobraStyles';

<Stack
  direction="column"
  spacing={2}
  padding={CobraStyles.Padding.MainWindow}
>
  {/* Page header */}
  <Stack direction="row" justifyContent="space-between" alignItems="center">
    <Typography variant="h4">My Checklists</Typography>
    <CobraNewButton onClick={handleCreate}>Create Checklist</CobraNewButton>
  </Stack>

  {/* Page content */}
  {/* ... */}
</Stack>
```

### Dialog Example

```tsx
import { Stack, DialogActions } from '@mui/material';
import {
  CobraDialog,
  CobraTextField,
  CobraSaveButton,
  CobraLinkButton
} from 'theme/styledComponents';
import CobraStyles from 'theme/CobraStyles';

<CobraDialog
  open={isOpen}
  onClose={handleClose}
  title="Create New Checklist"
  contentWidth="600px"
>
  <Stack spacing={CobraStyles.Spacing.FormFields}>
    <CobraTextField
      label="Checklist Name"
      value={name}
      onChange={(e) => setName(e.target.value)}
      fullWidth
      required
    />

    <CobraTextField
      label="Description"
      value={description}
      onChange={(e) => setDescription(e.target.value)}
      fullWidth
      multiline
      rows={3}
    />

    <DialogActions>
      <CobraLinkButton onClick={handleClose}>Cancel</CobraLinkButton>
      <CobraSaveButton onClick={handleSave} isSaving={saving}>
        Create
      </CobraSaveButton>
    </DialogActions>
  </Stack>
</CobraDialog>
```

---

## Theme Configuration

### Theme Dimensions

- **Header Height:** 54px (`theme.cssStyling.headerHeight`)
- **Drawer Closed Width:** 64px (`theme.cssStyling.drawerClosedWidth`)
- **Drawer Open Width:** 288px (`theme.cssStyling.drawerOpenWidth`)

### Component Defaults

All form controls default to **small** size:
- TextField, Autocomplete, Select
- Button, ButtonGroup, ToggleButtonGroup

### Typography

- **Font Family:** Roboto, Arial, sans-serif
- **Base Font Size:** 14px
- **Button Text:** No text transform (uses natural casing)

---

## Button Selection Guide

Use this table to choose the correct button component:

| Action Type | Component | Visual Style | Icon |
|-------------|-----------|--------------|------|
| Primary CTA | CobraPrimaryButton | Blue filled | Custom |
| Create new entity | CobraNewButton | Blue filled | Plus |
| Save with feedback | CobraSaveButton | Blue filled | Floppy disk / Spinner |
| Delete/remove | CobraDeleteButton | Red filled | Trash can |
| Alternative action | CobraSecondaryButton | Blue outlined | Custom |
| Cancel/dismiss | CobraLinkButton | Text (no background) | None |

---

## Migration Checklist

When converting a component to use COBRA styles:

- [ ] Replace all `Button` imports with appropriate COBRA button components
- [ ] Replace all `TextField` with `CobraTextField`
- [ ] Replace all `Checkbox` with `CobraCheckbox`
- [ ] Replace all `Dialog` with `CobraDialog`
- [ ] Replace hardcoded spacing values with `CobraStyles` constants
- [ ] Replace hardcoded colors with `theme.palette` references
- [ ] Ensure Stack spacing uses `CobraStyles.Spacing.*` values
- [ ] Ensure page padding uses `CobraStyles.Padding.MainWindow`
- [ ] Ensure dialog padding uses `CobraStyles.Padding.DialogContent`

---

## Validation Rules

### ❌ Common Mistakes

1. **Using plain MUI components**
   ```tsx
   // BAD
   import { Button, TextField } from '@mui/material';
   ```

2. **Hardcoded spacing**
   ```tsx
   // BAD
   <Stack spacing={2} padding="20px">
   ```

3. **Hardcoded colors**
   ```tsx
   // BAD
   <Box sx={{ backgroundColor: '#0020c2' }}>
   ```

4. **Wrong button for action**
   ```tsx
   // BAD - delete action using primary button
   <CobraPrimaryButton onClick={handleDelete}>Delete</CobraPrimaryButton>
   ```

### ✅ Best Practices

1. **Import from styledComponents**
   ```tsx
   // GOOD
   import { CobraPrimaryButton, CobraTextField } from 'theme/styledComponents';
   ```

2. **Use CobraStyles constants**
   ```tsx
   // GOOD
   <Stack
     spacing={CobraStyles.Spacing.FormFields}
     padding={CobraStyles.Padding.MainWindow}
   >
   ```

3. **Use theme palette**
   ```tsx
   // GOOD
   const theme = useTheme();
   <Box sx={{ backgroundColor: theme.palette.buttonPrimary.main }}>
   ```

4. **Match button to action**
   ```tsx
   // GOOD - delete action using delete button
   <CobraDeleteButton onClick={handleDelete}>Delete Template</CobraDeleteButton>
   ```

---

## File Structure

```
src/frontend/src/theme/
├── cobraTheme.ts                    # Main theme configuration
├── CobraStyles.ts                   # Spacing and padding constants
├── c5Theme.ts                       # Legacy theme (deprecated)
└── styledComponents/
    ├── index.ts                     # Central export file
    ├── CobraPrimaryButton.tsx
    ├── CobraSecondaryButton.tsx
    ├── CobraDeleteButton.tsx
    ├── CobraNewButton.tsx
    ├── CobraSaveButton.tsx
    ├── CobraLinkButton.tsx
    ├── CobraTextField.tsx
    ├── CobraCheckbox.tsx
    ├── CobraSwitch.tsx
    ├── CobraDivider.tsx
    └── CobraDialog.tsx
```

---

## Resources

- **Source Repository:** [cobra-styling-package](https://github.com/dynamisinc/cobra-styling-package)
- **Material-UI Docs:** [mui.com](https://mui.com/material-ui/getting-started/)
- **CLAUDE.md:** Project-specific AI assistant guide
- **UI_PATTERNS.md:** UX patterns and design philosophy

---

## FAQ

**Q: Can I use plain MUI components for one-off cases?**
A: No. Always use COBRA styled components for consistency across all COBRA applications.

**Q: What if I need a button style that doesn't exist?**
A: Use the closest matching COBRA button and customize via `sx` prop if absolutely necessary. Consider adding a new styled component if the pattern is reusable.

**Q: Can I override COBRA component styles?**
A: Yes, use the `sx` prop for component-specific overrides, but avoid overriding core colors or spacing.

**Q: How do I access the theme in a component?**
A: Use `const theme = useTheme()` from `@mui/material/styles`.

**Q: What about icons?**
A: Use FontAwesome icons (already installed). Import from `@fortawesome/free-solid-svg-icons` or `@fortawesome/free-regular-svg-icons`.

**Q: Is the old c5Theme.ts still valid?**
A: No. Use `cobraTheme.ts` instead. The old theme is deprecated and should not be used in new code.

---

## Change Log

| Date | Version | Changes |
|------|---------|---------|
| 2025-11-24 | 1.0.0 | Initial COBRA styling integration. Created cobraTheme.ts, CobraStyles.ts, and 11 styled components. |

---

**For questions or issues, refer to:**
- [COBRA Styling Package Documentation](https://github.com/dynamisinc/cobra-styling-package/tree/main/docs)
- CLAUDE.md for AI assistant guidelines
- UI_PATTERNS.md for UX principles
