/**
 * Admin Page - Central Administration Hub
 *
 * Provides access to administrative functions across all tools.
 * Only accessible to users with Manage role.
 *
 * Current sections:
 * - Checklists: Manage templates and checklist instances
 *
 * Future sections:
 * - Users: User management and permissions
 * - AI Providers: LLM configuration and management
 */

import React from "react";
import { useNavigate } from "react-router-dom";
import {
  Container,
  Stack,
  Box,
  Typography,
  Card,
  CardActionArea,
  Grid,
  Chip,
} from "@mui/material";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faGear,
  faClipboardList,
  faBoxArchive,
  faUsers,
  faBrain,
  faFileAlt,
  faToggleOn,
} from "@fortawesome/free-solid-svg-icons";
import { useTheme } from "@mui/material/styles";
import CobraStyles from "../theme/CobraStyles";
import { FeatureFlagsAdmin } from "../components/admin/FeatureFlagsAdmin";

interface AdminCardProps {
  icon: typeof faGear;
  title: string;
  description: string;
  path: string;
  disabled?: boolean;
}

const AdminCard: React.FC<AdminCardProps> = ({
  icon,
  title,
  description,
  path,
  disabled,
}) => {
  const navigate = useNavigate();
  const theme = useTheme();

  return (
    <Card
      sx={{
        height: "100%",
        opacity: disabled ? 0.5 : 1,
        "&:hover": disabled ? {} : { boxShadow: 4 },
      }}
    >
      <CardActionArea
        onClick={() => !disabled && navigate(path)}
        disabled={disabled}
        sx={{ height: "100%", p: 2 }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Box
            sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              backgroundColor: disabled
                ? theme.palette.grey[200]
                : theme.palette.buttonPrimary.light,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <FontAwesomeIcon
              icon={icon}
              size="lg"
              style={{
                color: disabled
                  ? theme.palette.grey[500]
                  : theme.palette.buttonPrimary.main,
              }}
            />
          </Box>
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" fontWeight={600}>
              {title}
              {disabled && (
                <Chip label="Coming Soon" size="small" sx={{ ml: 1 }} />
              )}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          </Box>
        </Box>
      </CardActionArea>
    </Card>
  );
};

interface AdminSectionProps {
  title: string;
  icon: typeof faGear;
  children: React.ReactNode;
}

const AdminSection: React.FC<AdminSectionProps> = ({ title, icon, children }) => {
  const theme = useTheme();

  return (
    <Box>
      <Typography
        variant="h6"
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1.5,
          mb: 2,
          color: theme.palette.text.primary,
        }}
      >
        <FontAwesomeIcon icon={icon} />
        {title}
      </Typography>
      {children}
    </Box>
  );
};

export const AdminPage: React.FC = () => {
  const theme = useTheme();

  // Checklist admin items
  const checklistAdminItems = [
    {
      icon: faFileAlt,
      title: "Manage Templates",
      description: "Create, edit, and organize checklist templates",
      path: "/checklists/manage",
      disabled: false,
    },
    {
      icon: faBoxArchive,
      title: "Manage Checklists",
      description: "Restore or permanently delete archived checklists",
      path: "/checklists/instances",
      disabled: false,
    },
  ];

  // Future admin sections
  const futureAdminItems = [
    {
      icon: faUsers,
      title: "User Management",
      description: "Manage users, roles, and permissions",
      path: "/admin/users",
      disabled: true,
    },
    {
      icon: faBrain,
      title: "AI Providers",
      description: "Configure LLM providers and AI settings",
      path: "/admin/ai-providers",
      disabled: true,
    },
  ];

  return (
    <Container maxWidth={false} disableGutters>
      <Stack spacing={4} padding={CobraStyles.Padding.MainWindow}>
        {/* Page Header */}
        <Box>
          <Typography
            variant="h4"
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 1.5,
              mb: 1,
            }}
          >
            <FontAwesomeIcon
              icon={faGear}
              style={{ color: theme.palette.buttonPrimary.main }}
            />
            Administration
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage system settings, templates, and configurations
          </Typography>
        </Box>

        {/* Feature Flags Section */}
        <AdminSection title="Feature Flags" icon={faToggleOn}>
          <FeatureFlagsAdmin />
        </AdminSection>

        {/* Checklists Section */}
        <AdminSection title="Checklists" icon={faClipboardList}>
          <Grid container spacing={2}>
            {checklistAdminItems.map((item) => (
              <Grid item xs={12} sm={6} md={4} key={item.title}>
                <AdminCard {...item} />
              </Grid>
            ))}
          </Grid>
        </AdminSection>

        {/* System Section (Future) */}
        <AdminSection title="System" icon={faGear}>
          <Grid container spacing={2}>
            {futureAdminItems.map((item) => (
              <Grid item xs={12} sm={6} md={4} key={item.title}>
                <AdminCard {...item} />
              </Grid>
            ))}
          </Grid>
        </AdminSection>
      </Stack>
    </Container>
  );
};

export default AdminPage;
