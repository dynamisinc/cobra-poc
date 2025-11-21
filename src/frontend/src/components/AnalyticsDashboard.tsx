import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
} from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFileLines,
  faListCheck,
  faBoxArchive,
  faChartLine,
  faStar,
  faTriangleExclamation,
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'react-toastify';
import { analyticsService } from '../services/analyticsService';
import type { AnalyticsDashboard as AnalyticsDashboardData } from '../types';
import { c5Colors } from '../theme/c5Theme';

/**
 * Analytics Dashboard Component
 *
 * Displays comprehensive analytics about templates and library items:
 * - Overview statistics
 * - Most used templates
 * - Never used templates
 * - Most popular library items
 * - Recently created templates
 */
export const AnalyticsDashboard: React.FC = () => {
  const [data, setData] = useState<AnalyticsDashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchAnalytics();
  }, []);

  const fetchAnalytics = async () => {
    try {
      setLoading(true);
      setError(null);
      const analytics = await analyticsService.getDashboard();
      setData(analytics);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load analytics';
      setError(message);
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error || 'Failed to load analytics'}
      </Alert>
    );
  }

  const { overview, mostUsedTemplates, neverUsedTemplates, mostPopularLibraryItems, recentlyCreatedTemplates } =
    data;

  return (
    <Box>
      {/* Overview Statistics */}
      <Typography variant="h6" sx={{ mb: 2 }}>
        Overview
      </Typography>
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card sx={{ backgroundColor: c5Colors.whiteBlue }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FontAwesomeIcon icon={faFileLines} style={{ color: c5Colors.cobaltBlue }} />
                <Typography variant="body2" color="text.secondary">
                  Templates
                </Typography>
              </Box>
              <Typography variant="h4">{overview.totalTemplates}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card sx={{ backgroundColor: c5Colors.whiteBlue }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FontAwesomeIcon icon={faListCheck} style={{ color: c5Colors.successGreen }} />
                <Typography variant="body2" color="text.secondary">
                  Active Templates
                </Typography>
              </Box>
              <Typography variant="h4">{overview.activeTemplates}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card sx={{ backgroundColor: c5Colors.whiteBlue }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FontAwesomeIcon icon={faTriangleExclamation} style={{ color: c5Colors.canaryYellow }} />
                <Typography variant="body2" color="text.secondary">
                  Unused Templates
                </Typography>
              </Box>
              <Typography variant="h4">{overview.unusedTemplates}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card sx={{ backgroundColor: c5Colors.whiteBlue }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FontAwesomeIcon icon={faChartLine} style={{ color: c5Colors.cobaltBlue }} />
                <Typography variant="body2" color="text.secondary">
                  Checklists Created
                </Typography>
              </Box>
              <Typography variant="h4">{overview.totalChecklistInstances}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card sx={{ backgroundColor: c5Colors.whiteBlue }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <FontAwesomeIcon icon={faBoxArchive} style={{ color: c5Colors.cobaltBlue }} />
                <Typography variant="body2" color="text.secondary">
                  Library Items
                </Typography>
              </Box>
              <Typography variant="h4">{overview.totalLibraryItems}</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Most Used Templates */}
      {mostUsedTemplates.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <FontAwesomeIcon icon={faStar} style={{ color: c5Colors.successGreen }} />
            Most Used Templates
          </Typography>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Template Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell align="right">Times Used</TableCell>
                  <TableCell>Created By</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {mostUsedTemplates.map((template) => (
                  <TableRow key={template.id} hover>
                    <TableCell>{template.name}</TableCell>
                    <TableCell>
                      <Chip label={template.category} size="small" />
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body1" sx={{ fontWeight: 'bold', color: c5Colors.successGreen }}>
                        {template.usageCount}
                      </Typography>
                    </TableCell>
                    <TableCell>{template.createdBy}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      {/* Most Popular Library Items */}
      {mostPopularLibraryItems.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <FontAwesomeIcon icon={faStar} style={{ color: c5Colors.cobaltBlue }} />
            Most Popular Library Items
          </Typography>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Item Text</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell align="right">Usage Count</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {mostPopularLibraryItems.map((item) => (
                  <TableRow key={item.id} hover>
                    <TableCell>{item.itemText}</TableCell>
                    <TableCell>
                      <Chip label={item.category} size="small" />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={item.itemType === 'checkbox' ? 'Checkbox' : 'Status'}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body1" sx={{ fontWeight: 'bold', color: c5Colors.cobaltBlue }}>
                        {item.usageCount}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      {/* Never Used Templates */}
      {neverUsedTemplates.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <FontAwesomeIcon icon={faTriangleExclamation} style={{ color: c5Colors.canaryYellow }} />
            Templates Never Used
          </Typography>
          <Alert severity="warning" sx={{ mb: 2 }}>
            These templates have never been used to create a checklist. Consider reviewing them for relevance or
            promoting their use.
          </Alert>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Template Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Created</TableCell>
                  <TableCell>Created By</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {neverUsedTemplates.map((template) => (
                  <TableRow key={template.id} hover>
                    <TableCell>{template.name}</TableCell>
                    <TableCell>
                      <Chip label={template.category} size="small" />
                    </TableCell>
                    <TableCell>{new Date(template.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell>{template.createdBy}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}

      {/* Recently Created Templates */}
      {recentlyCreatedTemplates.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Recently Created Templates
          </Typography>
          <Grid container spacing={2}>
            {recentlyCreatedTemplates.map((template) => (
              <Grid item xs={12} md={6} lg={4} key={template.id}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" sx={{ mb: 1 }}>
                      {template.name}
                    </Typography>
                    <Chip label={template.category} size="small" sx={{ mb: 1 }} />
                    {template.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        {template.description.length > 100
                          ? `${template.description.substring(0, 100)}...`
                          : template.description}
                      </Typography>
                    )}
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                      {template.items.length} items • Created by {template.createdBy}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(template.createdAt).toLocaleDateString()}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>
      )}
    </Box>
  );
};
