/**
 * AppHeader Component - Top Navigation Bar
 *
 * Implements C5-style header with:
 * - App branding (left)
 * - Event context display (future)
 * - Profile menu (right)
 * - Mobile menu toggle
 *
 * Height: 54px (theme.cssStyling.headerHeight)
 */

import React from "react";
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
} from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faClipboardList,
  faBars,
} from "@fortawesome/free-solid-svg-icons";
import { ProfileMenu } from "../ProfileMenu";
import { PermissionRole } from "../../types";

interface AppHeaderProps {
  onMobileMenuToggle?: () => void;
  onProfileChange?: (positions: string[], role: PermissionRole) => void;
}

export const AppHeader: React.FC<AppHeaderProps> = ({
  onMobileMenuToggle,
  onProfileChange,
}) => {
  const theme = useTheme();

  return (
    <AppBar
      position="fixed"
      sx={{
        backgroundColor: theme.palette.buttonPrimary.main,
        boxShadow: 2,
        height: theme.cssStyling.headerHeight,
        zIndex: theme.zIndex.drawer + 1,
      }}
    >
      <Toolbar
        sx={{
          minHeight: `${theme.cssStyling.headerHeight}px !important`,
          height: theme.cssStyling.headerHeight,
        }}
      >
        {/* Mobile Menu Toggle */}
        <IconButton
          color="inherit"
          aria-label="open drawer"
          edge="start"
          onClick={onMobileMenuToggle}
          sx={{
            mr: 2,
            display: { md: "none" },
            color: "#FFFACD",
          }}
        >
          <FontAwesomeIcon icon={faBars} />
        </IconButton>

        {/* App Logo/Name */}
        <Box sx={{ display: "flex", alignItems: "center" }}>
          <FontAwesomeIcon
            icon={faClipboardList}
            size="lg"
            style={{ marginRight: 12, color: "#FFFACD" }}
          />
          <Typography
            variant="h6"
            noWrap
            sx={{
              fontWeight: "bold",
              color: "#FFFACD",
              display: { xs: "none", sm: "block" },
            }}
          >
            COBRA Checklist
          </Typography>
        </Box>

        {/* Center: Event Context (future enhancement) */}
        <Box
          sx={{
            flexGrow: 1,
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
          }}
        >
          {/* Placeholder for event name badge like in C5 screenshot */}
          {/* Example: <Chip label="Hurricane Response 2025" color="warning" /> */}
        </Box>

        {/* Right: Profile Menu */}
        <Box sx={{ display: "flex", alignItems: "center" }}>
          {onProfileChange && (
            <ProfileMenu onProfileChange={onProfileChange} />
          )}
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default AppHeader;
