/**
 * Breadcrumb Component - Navigation Trail
 *
 * Implements C5-style breadcrumb navigation showing:
 * - Home / Events / [Event Name] / [Tool] / [View]
 *
 * For POC, we simplify to:
 * - Home / Checklist / [View] / [Detail Name]
 *
 * Features:
 * - Clickable links for navigation
 * - Current item (last) is not a link
 * - Light gray background per C5 theme
 */

import React from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Box, Typography, Link, Stack } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faHome } from "@fortawesome/free-solid-svg-icons";

export interface BreadcrumbItem {
  label: string;
  path?: string; // If undefined, item is not clickable (current page)
  icon?: typeof faHome;
}

interface BreadcrumbProps {
  items: BreadcrumbItem[];
}

export const Breadcrumb: React.FC<BreadcrumbProps> = ({ items }) => {
  const theme = useTheme();
  const navigate = useNavigate();

  const handleClick = (path?: string) => {
    if (path) {
      navigate(path);
    }
  };

  return (
    <Box
      sx={{
        backgroundColor: theme.palette.breadcrumb.background,
        px: 2,
        py: 1,
        borderBottom: `1px solid ${theme.palette.divider}`,
        minHeight: 40,
        display: "flex",
        alignItems: "center",
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        alignItems="center"
        sx={{ flexWrap: "wrap" }}
      >
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const isClickable = !!item.path && !isLast;

          return (
            <React.Fragment key={index}>
              {/* Separator (except for first item) */}
              {index > 0 && (
                <Typography
                  component="span"
                  sx={{
                    color: theme.palette.text.secondary,
                    fontSize: 12,
                    mx: 0.5,
                  }}
                >
                  /
                </Typography>
              )}

              {/* Breadcrumb Item */}
              {isClickable ? (
                <Link
                  component="button"
                  onClick={() => handleClick(item.path)}
                  sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 0.5,
                    color: theme.palette.buttonPrimary.main,
                    textDecoration: "none",
                    fontSize: 14,
                    fontWeight: 400,
                    cursor: "pointer",
                    "&:hover": {
                      textDecoration: "underline",
                    },
                  }}
                >
                  {item.icon && (
                    <FontAwesomeIcon icon={item.icon} size="sm" />
                  )}
                  {item.label}
                </Link>
              ) : (
                <Typography
                  component="span"
                  sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 0.5,
                    color: isLast
                      ? theme.palette.text.primary
                      : theme.palette.text.secondary,
                    fontSize: 14,
                    fontWeight: isLast ? 500 : 400,
                  }}
                >
                  {item.icon && (
                    <FontAwesomeIcon icon={item.icon} size="sm" />
                  )}
                  {item.label}
                </Typography>
              )}
            </React.Fragment>
          );
        })}
      </Stack>
    </Box>
  );
};

/**
 * Hook to generate breadcrumb items based on current route
 */
export const useBreadcrumbs = (
  customItems?: BreadcrumbItem[]
): BreadcrumbItem[] => {
  const location = useLocation();

  // If custom items provided, use them
  if (customItems && customItems.length > 0) {
    return customItems;
  }

  // Default breadcrumb generation based on route
  const pathParts = location.pathname.split("/").filter(Boolean);

  const items: BreadcrumbItem[] = [
    { label: "Home", path: "/", icon: faHome },
  ];

  // Build breadcrumbs based on path
  if (pathParts[0] === "checklists") {
    items.push({ label: "Checklist", path: "/checklists" });

    if (pathParts[1] === "manage") {
      items.push({ label: "Manage" }); // Current page, no path
    } else if (pathParts[1] === "analytics") {
      items.push({ label: "Analytics" }); // Current page, no path
    } else if (pathParts[1]) {
      // Checklist detail page - pathParts[1] is the checklist ID
      // The actual name will be set by the page component using customItems
      items.push({ label: "Dashboard", path: "/checklists" });
    } else {
      // /checklists (dashboard)
      items[items.length - 1] = { label: "Checklist" }; // Remove path to make it current
      items.push({ label: "Dashboard" }); // Current page
    }
  }

  return items;
};

export default Breadcrumb;
