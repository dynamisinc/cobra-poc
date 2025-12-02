/**
 * Shared Type Definitions
 *
 * App-wide types used across multiple modules.
 * Tool-specific types should be in their respective modules:
 * - Checklist types: tools/checklist/types/
 * - Event types: shared/events/types/
 */

// ============================================================================
// Permission & Auth Types
// ============================================================================

/**
 * Permission roles for access control
 */
export enum PermissionRole {
  NONE = 'None',
  READONLY = 'Readonly',
  CONTRIBUTOR = 'Contributor',
  MANAGE = 'Manage',
  SYSTEM_ADMIN = 'SystemAdmin', // Customer-level system administration
}

/**
 * Mock user for POC (replaces actual authentication)
 */
export interface MockUser {
  id: string;
  email: string;
  displayName: string;
  position: string; // Primary position
  positions: string[]; // All positions (for multi-position users)
  permissionRole: PermissionRole; // Access level
  eventId: string;
  eventName: string;
  eventCategory?: string; // Primary event category (e.g., "Fire", "Flood")
}

// ============================================================================
// ICS Constants
// ============================================================================

/**
 * Available ICS positions (for POC - normally from API)
 */
export const ICS_POSITIONS = [
  'Incident Commander',
  'Operations Section Chief',
  'Planning Section Chief',
  'Logistics Section Chief',
  'Finance/Admin Section Chief',
  'Safety Officer',
  'Public Information Officer',
  'Liaison Officer',
] as const;

export type ICSPosition = typeof ICS_POSITIONS[number];

/**
 * ICS Standard Incident Types (for POC - will come from C5 in production)
 * Based on FEMA Incident Types
 */
export const ICS_INCIDENT_TYPES = [
  'Hurricane',
  'Flood',
  'Wildfire',
  'Earthquake',
  'Tornado',
  'Winter Storm',
  'Hazmat',
  'Search and Rescue',
  'Mass Casualty',
  'Civil Unrest',
  'Cyber Incident',
  'Infrastructure Failure',
  'Public Health Emergency',
  'Terrorism',
  'Other',
] as const;

export type ICSIncidentType = typeof ICS_INCIDENT_TYPES[number];

// ============================================================================
// Generic UI State Types
// ============================================================================

/**
 * Sort options for lists
 */
export interface SortOptions {
  field: string;
  direction: 'asc' | 'desc';
}

/**
 * Pagination state
 */
export interface PaginationState {
  page: number;
  pageSize: number;
  totalItems: number;
}

/**
 * Loading state for async operations
 */
export interface LoadingState {
  isLoading: boolean;
  error: string | null;
}

/**
 * Toast notification type
 */
export interface ToastNotification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number; // milliseconds
}

/**
 * Form validation error
 */
export interface ValidationError {
  field: string;
  message: string;
}

// ============================================================================
// Generic API Types
// ============================================================================

/**
 * API Response wrapper
 */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

/**
 * API Error response
 */
export interface ApiError {
  message: string;
  statusCode: number;
  errors?: Record<string, string[]>; // Validation errors
}

// ============================================================================
// Re-exports for backward compatibility
// ============================================================================

// Re-export checklist types from their new location
export type {
  Template,
  TemplateItem,
  StatusOption,
  ChecklistInstance,
  ChecklistItem,
  ItemStatusHistory,
  ItemNote,
  CreateTemplateRequest,
  CreateTemplateItemRequest,
  UpdateTemplateRequest,
  UpdateTemplateItemRequest,
  CreateChecklistRequest,
  CreateCombinedChecklistRequest,
  UpdateItemCompletionRequest,
  UpdateItemStatusRequest,
  AddItemNoteRequest,
  ReorderItemsRequest,
  BulkUpdateItemsRequest,
  TemplateFilters,
  ChecklistFilters,
  ItemCompletedMessage,
  ItemStatusChangedMessage,
  ItemNoteAddedMessage,
  ChecklistUpdatedMessage,
  TemplateFormState,
  TemplateItemFormState,
  ItemLibraryEntry,
  CreateItemLibraryEntryRequest,
  UpdateItemLibraryEntryRequest,
  AnalyticsDashboard,
  AnalyticsOverview,
  TemplateUsage,
  ItemLibraryUsage,
} from '../tools/checklist/types';

export {
  TemplateCategory,
  TemplateType,
  ItemType,
  BulkAction,
  DEFAULT_STATUS_OPTIONS,
} from '../tools/checklist/types';

// Re-export event types from their location
export type {
  EventType,
  EventCategorySubGroup,
  EventCategory,
  Event,
  CreateEventRequest,
  UpdateEventRequest,
} from '../shared/events/types';
