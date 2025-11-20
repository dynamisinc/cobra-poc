/**
 * App Component - Main Application Entry Point
 *
 * Sets up routing and global layout for the COBRA Checklist POC.
 * Uses BrowserRouter for clean URLs (not hash routing).
 */

import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Box, AppBar, Toolbar, Typography } from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faClipboardList } from '@fortawesome/free-solid-svg-icons';
import { MyChecklistsPage } from './pages/MyChecklistsPage';
import { ChecklistDetailPage } from './pages/ChecklistDetailPage';
import { PositionSelector } from './components/PositionSelector';
import { c5Colors } from './theme/c5Theme';

interface AppNavBarProps {
  onPositionChange: (positions: string[]) => void;
}

/**
 * App Navigation Bar
 */
const AppNavBar: React.FC<AppNavBarProps> = ({ onPositionChange }) => {
  return (
    <AppBar
      position="static"
      sx={{
        backgroundColor: c5Colors.cobaltBlue,
        boxShadow: 2,
      }}
    >
      <Toolbar>
        <FontAwesomeIcon
          icon={faClipboardList}
          size="lg"
          style={{ marginRight: 16 }}
        />
        <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
          COBRA Checklist POC
        </Typography>
        <PositionSelector onPositionChange={onPositionChange} />
      </Toolbar>
    </AppBar>
  );
};

/**
 * Main App Component
 */
function App() {
  // State to trigger re-renders when position changes
  const [positionKey, setPositionKey] = useState(0);

  /**
   * Handle position change from PositionSelector
   * Triggers re-render of pages to fetch new data
   */
  const handlePositionChange = (positions: string[]) => {
    console.log('[App] Position changed to:', positions);
    // Increment key to force re-render of routes
    setPositionKey((prev) => prev + 1);
  };

  return (
    <BrowserRouter>
      <Box sx={{ minHeight: '100vh', backgroundColor: '#F5F5F5' }}>
        <AppNavBar onPositionChange={handlePositionChange} />

        <Routes key={positionKey}>
          {/* Default route - redirect to My Checklists */}
          <Route path="/" element={<Navigate to="/checklists" replace />} />

          {/* My Checklists page */}
          <Route path="/checklists" element={<MyChecklistsPage />} />

          {/* Checklist Detail page */}
          <Route
            path="/checklists/:checklistId"
            element={<ChecklistDetailPage />}
          />

          {/* Catch-all route - redirect to My Checklists */}
          <Route path="*" element={<Navigate to="/checklists" replace />} />
        </Routes>
      </Box>
    </BrowserRouter>
  );
}

export default App;
