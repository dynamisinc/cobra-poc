/**
 * Sidebar Component - Collapsible Left Navigation
 *
 * Implements C5-style left sidebar with:
 * - Collapsible icon rail (64px closed, 288px open)
 * - Tool navigation items
 * - Role-based visibility
 *
 * Based on C5 Logbook/Dashboard navigation pattern.
 */

import React from "react";
import { useNavigate, useLocation } from "react-router-dom";
import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Typography,
  Tooltip,
  Divider,
  useMediaQuery,
} from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faClipboardList,
  faChevronLeft,
  faChevronRight,
  faGear,
  faChartLine,
  faHome,
} from "@fortawesome/free-solid-svg-icons";
import { usePermissions } from "../../hooks/usePermissions";

interface NavItem {
  id: string;
  label: string;
  icon: typeof faClipboardList;
  path: string;
  requiredPermission?: "canViewTemplateLibrary" | "canAccessItemLibrary";
  dividerAfter?: boolean;
}

interface SidebarProps {
  open: boolean;
  onToggle: () => void;
  mobileOpen?: boolean;
  onMobileClose?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  open,
  onToggle,
  mobileOpen = false,
  onMobileClose,
}) => {
  const theme = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const permissions = usePermissions();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  const drawerWidth = open
    ? theme.cssStyling.drawerOpenWidth
    : theme.cssStyling.drawerClosedWidth;

  // Navigation items - currently just Checklist tool, expandable for future
  const navItems: NavItem[] = [
    {
      id: "home",
      label: "Home",
      icon: faHome,
      path: "/",
      dividerAfter: true,
    },
    {
      id: "checklist-dashboard",
      label: "Dashboard",
      icon: faClipboardList,
      path: "/checklists",
    },
    {
      id: "checklist-manage",
      label: "Manage",
      icon: faGear,
      path: "/checklists/manage",
      requiredPermission: "canViewTemplateLibrary",
    },
    {
      id: "checklist-analytics",
      label: "Analytics",
      icon: faChartLine,
      path: "/checklists/analytics",
    },
  ];

  const handleNavClick = (path: string) => {
    navigate(path);
    if (isMobile && onMobileClose) {
      onMobileClose();
    }
  };

  const isActive = (path: string) => {
    if (path === "/checklists") {
      // Dashboard is active for /checklists and /checklists/:id but not /checklists/manage
      return (
        location.pathname === "/checklists" ||
        (location.pathname.startsWith("/checklists/") &&
          !location.pathname.includes("/manage") &&
          !location.pathname.includes("/analytics"))
      );
    }
    return location.pathname.startsWith(path);
  };

  const drawerContent = (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        height: "100%",
        backgroundColor: theme.palette.background.paper,
      }}
    >
      {/* Sidebar Header */}
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: open ? "space-between" : "center",
          px: open ? 2 : 0,
          py: 1.5,
          minHeight: theme.cssStyling.headerHeight,
          borderBottom: `1px solid ${theme.palette.divider}`,
          backgroundColor: theme.palette.primary.main,
        }}
      >
        {open && (
          <Typography
            variant="subtitle1"
            sx={{
              fontWeight: 600,
              color: theme.palette.primary.dark,
              whiteSpace: "nowrap",
              overflow: "hidden",
              textOverflow: "ellipsis",
            }}
          >
            Checklist Tool
          </Typography>
        )}
        <IconButton
          onClick={onToggle}
          size="small"
          sx={{
            color: theme.palette.primary.dark,
            "&:hover": {
              backgroundColor: theme.palette.primary.light,
            },
          }}
        >
          <FontAwesomeIcon
            icon={open ? faChevronLeft : faChevronRight}
            size="sm"
          />
        </IconButton>
      </Box>

      {/* Tools Section Label */}
      {open && (
        <Typography
          variant="overline"
          sx={{
            px: 2,
            pt: 2,
            pb: 1,
            color: theme.palette.text.secondary,
            fontWeight: 600,
            letterSpacing: 1,
          }}
        >
          Tools
        </Typography>
      )}

      {/* Navigation Items */}
      <List sx={{ flex: 1, pt: open ? 0 : 2 }}>
        {navItems.map((item) => {
          // Check permission if required
          if (
            item.requiredPermission &&
            !permissions[item.requiredPermission]
          ) {
            return null;
          }

          const active = isActive(item.path);

          return (
            <React.Fragment key={item.id}>
              <ListItem disablePadding sx={{ display: "block" }}>
                <Tooltip
                  title={!open ? item.label : ""}
                  placement="right"
                  arrow
                >
                  <ListItemButton
                    onClick={() => handleNavClick(item.path)}
                    sx={{
                      minHeight: 48,
                      justifyContent: open ? "initial" : "center",
                      px: 2.5,
                      mx: open ? 1 : 0.5,
                      my: 0.5,
                      borderRadius: open ? 1 : "50%",
                      backgroundColor: active
                        ? theme.palette.grid.main
                        : "transparent",
                      borderLeft: active && open ? `3px solid ${theme.palette.buttonPrimary.main}` : "none",
                      "&:hover": {
                        backgroundColor: active
                          ? theme.palette.grid.main
                          : theme.palette.grid.light,
                      },
                    }}
                  >
                    <ListItemIcon
                      sx={{
                        minWidth: 0,
                        mr: open ? 2 : "auto",
                        justifyContent: "center",
                        color: active
                          ? theme.palette.buttonPrimary.main
                          : theme.palette.text.primary,
                      }}
                    >
                      <FontAwesomeIcon icon={item.icon} />
                    </ListItemIcon>
                    {open && (
                      <ListItemText
                        primary={item.label}
                        primaryTypographyProps={{
                          fontSize: 14,
                          fontWeight: active ? 600 : 400,
                          color: active
                            ? theme.palette.buttonPrimary.main
                            : theme.palette.text.primary,
                        }}
                      />
                    )}
                  </ListItemButton>
                </Tooltip>
              </ListItem>
              {item.dividerAfter && (
                <Divider sx={{ my: 1, mx: open ? 2 : 1 }} />
              )}
            </React.Fragment>
          );
        })}
      </List>

      {/* Future: Additional sections can be added here */}
    </Box>
  );

  // Mobile drawer (temporary)
  if (isMobile) {
    return (
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={onMobileClose}
        ModalProps={{
          keepMounted: true, // Better mobile performance
        }}
        sx={{
          display: { xs: "block", md: "none" },
          "& .MuiDrawer-paper": {
            boxSizing: "border-box",
            width: theme.cssStyling.drawerOpenWidth,
          },
        }}
      >
        {drawerContent}
      </Drawer>
    );
  }

  // Desktop drawer (permanent)
  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          boxSizing: "border-box",
          borderRight: `1px solid ${theme.palette.divider}`,
          transition: theme.transitions.create("width", {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen,
          }),
          overflowX: "hidden",
        },
      }}
    >
      {drawerContent}
    </Drawer>
  );
};

export default Sidebar;
