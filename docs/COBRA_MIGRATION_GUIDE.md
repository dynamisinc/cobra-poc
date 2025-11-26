# COBRA Styling Migration Guide

**Date:** 2025-11-24
**Status:** In Progress

## Overview

This guide provides step-by-step instructions for migrating existing components to use COBRA styled components. Use this as a reference when updating the remaining components in the application.

## ✅ Completed Migrations

### 1. CreateChecklistDialog ✅
- **File:** `src/frontend/src/components/CreateChecklistDialog.tsx`
- **Changes:**
  - Replaced `Dialog` → `CobraDialog`
  - Replaced `TextField` → `CobraTextField`
  - Replaced `Button` (Cancel) → `CobraLinkButton`
  - Replaced `Button` (Save) → `CobraSaveButton` with `isSaving` prop
  - Replaced `Button` (Advanced) → `CobraSecondaryButton`
  - Added `Stack` with `CobraStyles.Spacing.FormFields`
  - Replaced hardcoded background color with `theme.palette.background.default`
  - Removed manual padding/spacing in favor of CobraStyles

### 2. ItemNotesDialog ✅
- **File:** `src/frontend/src/components/ItemNotesDialog.tsx`
- **Changes:**
  - Replaced `Dialog` → `CobraDialog`
  - Replaced `TextField` → `CobraTextField`
  - Replaced `Button` (Cancel) → `CobraLinkButton`
  - Replaced `Button` (Save) → `CobraSaveButton` with `isSaving` prop
  - Added `Stack` with `CobraStyles.Spacing.FormFields`
  - Replaced hardcoded background color with `theme.palette.background.default`

## 🔄 Migration Pattern

Use this pattern for all component migrations:

### Step 1: Update Imports

**Before:**
```tsx
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  // other MUI components
} from '@mui/material';
```

**After:**
```tsx
import {
  DialogActions,  // Keep only what's still needed
  Stack,          // Add Stack for layout
  // other MUI components that don't have COBRA equivalents
} from '@mui/material';
import {
  CobraDialog,
  CobraTextField,
  CobraSaveButton,
  CobraLinkButton,
  CobraPrimaryButton,
  CobraSecondaryButton,
  CobraDeleteButton,
} from '../theme/styledComponents';
import CobraStyles from '../theme/CobraStyles';
```

### Step 2: Replace Dialog Structure

**Before:**
```tsx
<Dialog
  open={open}
  onClose={handleClose}
  maxWidth="sm"
  fullWidth
>
  <DialogTitle>
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <FontAwesomeIcon icon={someIcon} />
      <Typography variant="h6">Dialog Title</Typography>
    </Box>
  </DialogTitle>

  <DialogContent>
    {/* content */}
  </DialogContent>

  <DialogActions>
    {/* buttons */}
  </DialogActions>
</Dialog>
```

**After:**
```tsx
<CobraDialog
  open={open}
  onClose={handleClose}
  title="Dialog Title"
  contentWidth="600px"  // or "400px", "800px", etc.
>
  <Stack spacing={CobraStyles.Spacing.FormFields}>
    {/* content */}

    <DialogActions>
      {/* buttons */}
    </DialogActions>
  </Stack>
</CobraDialog>
```

### Step 3: Replace TextField Components

**Before:**
```tsx
<TextField
  fullWidth
  label="Field Name"
  value={value}
  onChange={onChange}
  sx={{ mb: 2 }}
/>
```

**After:**
```tsx
<CobraTextField
  fullWidth
  label="Field Name"
  value={value}
  onChange={onChange}
  // No sx prop needed - spacing handled by Stack
/>
```

### Step 4: Replace Buttons

**Before:**
```tsx
<DialogActions sx={{ px: 3, pb: 2 }}>
  <Button variant="text" onClick={handleCancel}>
    Cancel
  </Button>
  <Button variant="contained" onClick={handleSave} disabled={saving}>
    {saving ? 'Saving...' : 'Save'}
  </Button>
</DialogActions>
```

**After:**
```tsx
<DialogActions>
  <CobraLinkButton onClick={handleCancel} disabled={saving}>
    Cancel
  </CobraLinkButton>
  <CobraSaveButton onClick={handleSave} isSaving={saving}>
    Save
  </CobraSaveButton>
</DialogActions>
```

### Step 5: Replace Hardcoded Colors

**Before:**
```tsx
<Box
  sx={{
    backgroundColor: '#F5F5F5',
    color: '#1a1a1a',
  }}
>
```

**After:**
```tsx
<Box
  sx={{
    backgroundColor: (theme) => theme.palette.background.default,
    color: (theme) => theme.palette.text.primary,
  }}
>
```

### Step 6: Replace Hardcoded Spacing

**Before:**
```tsx
<Box sx={{ mb: 2, mt: 3, gap: 2 }}>
  <TextField sx={{ mb: 2 }} />
  <TextField sx={{ mb: 2 }} />
</Box>
```

**After:**
```tsx
<Stack spacing={CobraStyles.Spacing.FormFields}>
  <CobraTextField />
  <CobraTextField />
</Stack>
```

## 📋 Component Migration Checklist

Use this checklist when migrating a component:

- [ ] Update imports: Remove plain MUI Button, TextField, Dialog
- [ ] Add COBRA component imports
- [ ] Add CobraStyles import
- [ ] Replace Dialog with CobraDialog
- [ ] Simplify title (no need for Box/Typography wrapper)
- [ ] Wrap content in Stack with CobraStyles.Spacing.FormFields
- [ ] Replace all TextField with CobraTextField
- [ ] Replace Cancel button with CobraLinkButton
- [ ] Replace Save button with CobraSaveButton (add isSaving prop)
- [ ] Replace Primary button with CobraPrimaryButton
- [ ] Replace Delete button with CobraDeleteButton
- [ ] Replace Secondary button with CobraSecondaryButton
- [ ] Remove hardcoded spacing (mb, mt, gap, padding)
- [ ] Replace hardcoded colors with theme.palette
- [ ] Remove unused imports
- [ ] Run type check: `npm run type-check`
- [ ] Test component in browser

## 🎯 Button Selection Guide

| Old Pattern | New Component | Notes |
|-------------|---------------|-------|
| `<Button variant="contained">Save</Button>` | `<CobraSaveButton isSaving={saving}>` | Use for save actions |
| `<Button variant="contained">Create</Button>` | `<CobraPrimaryButton>` | Use for create/submit |
| `<Button variant="contained">Delete</Button>` | `<CobraDeleteButton>` | Includes trash icon |
| `<Button variant="contained">New</Button>` | `<CobraNewButton>` | Includes plus icon |
| `<Button variant="outlined">` | `<CobraSecondaryButton>` | Alternative actions |
| `<Button variant="text">Cancel</Button>` | `<CobraLinkButton>` | Cancel/dismiss |

## 📁 Remaining Components to Migrate

### High Priority Dialogs (Similar to Completed)
- [ ] `ItemStatusDialog.tsx` - Status selection dialog
- [ ] `PositionSelector.tsx` - Position picker
- [ ] `TemplatePickerDialog.tsx` - Template selection
- [ ] `AddFromLibraryDialog.tsx` - Library item picker
- [ ] `SaveToLibraryDialog.tsx` - Save to library
- [ ] `ItemLibraryItemDialog.tsx` - Library item details

### Medium Priority Components
- [ ] `ChecklistFilters.tsx` - Filter controls
- [ ] `ChecklistCard.tsx` - Checklist display card
- [ ] `StatusConfigurationBuilder.tsx` - Status builder
- [ ] `TemplateItemEditor.tsx` - Template item form
- [ ] `SectionHeader.tsx` - Section headers
- [ ] `LibraryItemBrowser.tsx` - Library browser
- [ ] `AnalyticsDashboard.tsx` - Dashboard widgets

### Special Cases
- [ ] `ProfileMenu.tsx` - Menu component (may not need full migration)
- [ ] `BottomSheet.tsx` - Mobile bottom sheet (may need custom approach)

### Pages (After Components)
- [ ] `MyChecklistsPage.tsx`
- [ ] `ChecklistDetailPage.tsx`
- [ ] `TemplateLibraryPage.tsx`
- [ ] `TemplateEditorPage.tsx`
- [ ] `TemplatePreviewPage.tsx`
- [ ] `ItemLibraryPage.tsx`

## 🔍 Testing After Migration

After migrating each component:

1. **Type Check:**
   ```bash
   cd src/frontend
   npm run type-check
   ```

2. **Visual Test:**
   - Open the component in the browser
   - Verify buttons have rounded corners (50px border-radius)
   - Verify cobalt blue (#0020c2) for primary actions
   - Verify red (#e42217) for delete actions
   - Verify form fields have blue focus state
   - Verify spacing is consistent

3. **Functional Test:**
   - Test all button actions
   - Test form submission
   - Test validation
   - Test loading states (isSaving)

## ⚠️ Common Pitfalls

1. **Forgetting to remove unused imports**
   - TypeScript will warn about unused imports
   - Remove them to keep code clean

2. **Not using isSaving prop**
   - CobraSaveButton has built-in loading state
   - Use `isSaving` instead of checking in children

3. **Mixing spacing approaches**
   - Don't use both Stack spacing AND sx={{ mb: 2 }}
   - Let Stack handle all spacing

4. **Hardcoding colors**
   - Always use theme.palette
   - Never use hex values directly

5. **Wrong button for action**
   - Delete actions = CobraDeleteButton
   - Save actions = CobraSaveButton
   - Cancel actions = CobraLinkButton

## 📖 Resources

- **COBRA Styling Integration:** [COBRA_STYLING_INTEGRATION.md](./COBRA_STYLING_INTEGRATION.md)
- **CLAUDE.md:** [CLAUDE.md](../CLAUDE.md#cobra-styling-system)
- **Theme Reference:** [cobraTheme.ts](../src/frontend/src/theme/cobraTheme.ts)
- **Styled Components:** [styledComponents/](../src/frontend/src/theme/styledComponents/)

## 📊 Migration Progress

**Completed:** 2 / 23 components (9%)
- ✅ CreateChecklistDialog
- ✅ ItemNotesDialog

**In Progress:** This migration guide

**Remaining:** 21 components + 6 pages

---

**Next Steps:**
1. Continue with high-priority dialogs (ItemStatusDialog, PositionSelector, etc.)
2. Migrate medium-priority components
3. Update page components
4. Final visual review and testing

**Estimated Time:**
- High-priority dialogs: ~30 minutes per component
- Medium-priority: ~20 minutes per component
- Pages: ~45 minutes per page
- **Total:** ~8-10 hours of work

---

**For questions or issues, refer to:**
- [COBRA_STYLING_INTEGRATION.md](./COBRA_STYLING_INTEGRATION.md)
- [CLAUDE.md](../CLAUDE.md#cobra-styling-system)
