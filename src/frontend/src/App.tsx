/**
 * App Component - Main Application Entry Point
 *
 * Implements C5-style navigation with:
 * - Collapsible left sidebar
 * - Breadcrumb navigation
 * - Hierarchical routing under /checklists/*
 *
 * Route Structure:
 * - /checklists - Dashboard (My Checklists)
 * - /checklists/:id - Checklist Detail
 * - /checklists/manage - Templates & Item Library (Manage role)
 * - /checklists/manage/templates/new - Create Template
 * - /checklists/manage/templates/:id/edit - Edit Template
 * - /checklists/manage/templates/:id/preview - Preview Template
 * - /checklists/manage/templates/:id/duplicate - Duplicate Template
 * - /checklists/analytics - Analytics Dashboard (future)
 */

import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Box, Typography, Container, Stack } from "@mui/material";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine } from "@fortawesome/free-solid-svg-icons";

// Navigation components
import { AppLayout, BreadcrumbItem } from "./components/navigation";

// Pages
import { LandingPage } from "./pages/LandingPage";
import { ChecklistDetailPage } from "./pages/ChecklistDetailPage";
import { ManagePage } from "./pages/ManagePage";
import { TemplateEditorPage } from "./pages/TemplateEditorPage";
import { TemplatePreviewPage } from "./pages/TemplatePreviewPage";

// Styles
import CobraStyles from "./theme/CobraStyles";

/**
 * Dashboard Page Wrapper
 * Wraps LandingPage (My Checklists) with AppLayout
 */
const DashboardPage: React.FC = () => {
  const breadcrumbs: BreadcrumbItem[] = [
    { label: "Home", path: "/" },
    { label: "Checklist", path: "/checklists" },
    { label: "Dashboard" },
  ];

  return (
    <AppLayout breadcrumbs={breadcrumbs}>
      <LandingPage />
    </AppLayout>
  );
};

/**
 * Checklist Detail Page Wrapper
 * Wraps ChecklistDetailPage with AppLayout
 */
const ChecklistDetailWrapper: React.FC = () => {
  // Breadcrumbs will be set by the ChecklistDetailPage itself
  // since it needs to know the checklist name
  return <ChecklistDetailPage />;
};

/**
 * Template Editor Page Wrapper
 * Wraps TemplateEditorPage with AppLayout
 */
const TemplateEditorWrapper: React.FC<{ mode: "new" | "edit" | "duplicate" }> = ({
  mode,
}) => {
  const getTitle = () => {
    switch (mode) {
      case "new":
        return "Create Template";
      case "duplicate":
        return "Duplicate Template";
      case "edit":
        return "Edit Template";
    }
  };

  const breadcrumbs: BreadcrumbItem[] = [
    { label: "Home", path: "/" },
    { label: "Checklist", path: "/checklists" },
    { label: "Manage", path: "/checklists/manage?tab=templates" },
    { label: getTitle() },
  ];

  return (
    <AppLayout breadcrumbs={breadcrumbs}>
      <TemplateEditorPage />
    </AppLayout>
  );
};

/**
 * Template Preview Page Wrapper
 * Wraps TemplatePreviewPage with AppLayout
 */
const TemplatePreviewWrapper: React.FC = () => {
  const breadcrumbs: BreadcrumbItem[] = [
    { label: "Home", path: "/" },
    { label: "Checklist", path: "/checklists" },
    { label: "Manage", path: "/checklists/manage?tab=templates" },
    { label: "Preview" },
  ];

  return (
    <AppLayout breadcrumbs={breadcrumbs}>
      <TemplatePreviewPage />
    </AppLayout>
  );
};

/**
 * Analytics Page (Placeholder)
 */
const AnalyticsPage: React.FC = () => {
  const breadcrumbs: BreadcrumbItem[] = [
    { label: "Home", path: "/" },
    { label: "Checklist", path: "/checklists" },
    { label: "Analytics" },
  ];

  return (
    <AppLayout breadcrumbs={breadcrumbs}>
      <Container maxWidth="lg">
        <Stack
          spacing={3}
          padding={CobraStyles.Padding.MainWindow}
          sx={{ textAlign: "center", py: 8 }}
        >
          <FontAwesomeIcon
            icon={faChartLine}
            size="4x"
            style={{ color: "#c0c0c0" }}
          />
          <Typography variant="h4" color="text.secondary">
            Analytics Dashboard
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Coming soon - Progress tracking, trends, and workload analysis
          </Typography>
        </Stack>
      </Container>
    </AppLayout>
  );
};

/**
 * Home Page - Redirects to dashboard
 */
const HomePage: React.FC = () => {
  const breadcrumbs: BreadcrumbItem[] = [{ label: "Home" }];

  return (
    <AppLayout breadcrumbs={breadcrumbs}>
      <Container maxWidth="lg">
        <Stack spacing={3} padding={CobraStyles.Padding.MainWindow}>
          <Typography variant="h4">COBRA Checklist Tool</Typography>
          <Typography variant="body1" color="text.secondary">
            Welcome to the COBRA Checklist POC. Use the sidebar to navigate.
          </Typography>
          <Box sx={{ mt: 4 }}>
            <Typography variant="h6" gutterBottom>
              Quick Links
            </Typography>
            <Stack spacing={1}>
              <Typography>
                • <a href="/checklists">Dashboard</a> - View your checklists
              </Typography>
              <Typography>
                • <a href="/checklists/manage">Manage</a> - Templates & Item
                Library
              </Typography>
              <Typography>
                • <a href="/checklists/analytics">Analytics</a> - Progress &
                Trends
              </Typography>
            </Stack>
          </Box>
        </Stack>
      </Container>
    </AppLayout>
  );
};

/**
 * Main App Component
 */
function App() {
  return (
    <BrowserRouter>
      <Box sx={{ minHeight: "100vh", backgroundColor: "#F5F5F5" }}>
        <Routes>
          {/* Home page */}
          <Route path="/" element={<HomePage />} />

          {/* Checklist Dashboard (My Checklists) */}
          <Route path="/checklists" element={<DashboardPage />} />

          {/* Checklist Detail */}
          <Route
            path="/checklists/:checklistId"
            element={<ChecklistDetailWrapper />}
          />

          {/* Manage Section (Templates & Item Library) */}
          <Route path="/checklists/manage" element={<ManagePage />} />

          {/* Template Management Routes */}
          <Route
            path="/checklists/manage/templates/new"
            element={<TemplateEditorWrapper mode="new" />}
          />
          <Route
            path="/checklists/manage/templates/:templateId/edit"
            element={<TemplateEditorWrapper mode="edit" />}
          />
          <Route
            path="/checklists/manage/templates/:templateId/preview"
            element={<TemplatePreviewWrapper />}
          />
          <Route
            path="/checklists/manage/templates/:templateId/duplicate"
            element={<TemplateEditorWrapper mode="duplicate" />}
          />

          {/* Analytics Page */}
          <Route path="/checklists/analytics" element={<AnalyticsPage />} />

          {/* Legacy routes - redirect to new structure */}
          <Route
            path="/templates"
            element={<Navigate to="/checklists/manage?tab=templates" replace />}
          />
          <Route
            path="/templates/new"
            element={<Navigate to="/checklists/manage/templates/new" replace />}
          />
          <Route
            path="/templates/:templateId/edit"
            element={
              <Navigate to="/checklists/manage/templates/:templateId/edit" replace />
            }
          />
          <Route
            path="/templates/:templateId/preview"
            element={
              <Navigate to="/checklists/manage/templates/:templateId/preview" replace />
            }
          />
          <Route
            path="/templates/:templateId/duplicate"
            element={
              <Navigate to="/checklists/manage/templates/:templateId/duplicate" replace />
            }
          />
          <Route
            path="/item-library"
            element={<Navigate to="/checklists/manage?tab=items" replace />}
          />

          {/* Catch-all - redirect to dashboard */}
          <Route path="*" element={<Navigate to="/checklists" replace />} />
        </Routes>
      </Box>
    </BrowserRouter>
  );
}

export default App;
