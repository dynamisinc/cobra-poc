/**
 * Feature Flags Type Definitions
 *
 * Controls which POC tools are enabled/visible in the application.
 * Flags are loaded from the API (appsettings.json defaults + database overrides).
 */

export interface FeatureFlags {
  /** Checklist tool - create and manage operational checklists */
  checklist: boolean;
  /** External Chat integration - GroupMe/messaging platform integration */
  chat: boolean;
  /** Tasking tool - task assignment and tracking */
  tasking: boolean;
  /** COBRA KAI - knowledge and intelligence assistant */
  cobraKai: boolean;
  /** Event Summary - event overview and reporting */
  eventSummary: boolean;
  /** Status Chart - visual status tracking */
  statusChart: boolean;
  /** Event Timeline - chronological event view */
  eventTimeline: boolean;
  /** COBRA AI - AI-powered assistance */
  cobraAi: boolean;
}

/**
 * Feature flag metadata for admin UI display
 */
export interface FeatureFlagInfo {
  key: keyof FeatureFlags;
  name: string;
  description: string;
  category: 'core' | 'communication' | 'visualization' | 'ai';
}

/**
 * All available feature flags with display metadata
 */
export const featureFlagInfo: FeatureFlagInfo[] = [
  {
    key: 'checklist',
    name: 'Checklist',
    description: 'Create and manage operational checklists from templates',
    category: 'core',
  },
  {
    key: 'chat',
    name: 'External Chat',
    description: 'GroupMe and external messaging platform integration',
    category: 'communication',
  },
  {
    key: 'tasking',
    name: 'Tasking',
    description: 'Task assignment and tracking for team members',
    category: 'core',
  },
  {
    key: 'cobraKai',
    name: 'COBRA KAI',
    description: 'Knowledge and intelligence assistant',
    category: 'ai',
  },
  {
    key: 'eventSummary',
    name: 'Event Summary',
    description: 'Event overview and executive reporting',
    category: 'visualization',
  },
  {
    key: 'statusChart',
    name: 'Status Chart',
    description: 'Visual status tracking dashboard',
    category: 'visualization',
  },
  {
    key: 'eventTimeline',
    name: 'Event Timeline',
    description: 'Chronological view of event activities',
    category: 'visualization',
  },
  {
    key: 'cobraAi',
    name: 'COBRA AI',
    description: 'AI-powered assistance and recommendations',
    category: 'ai',
  },
];

/**
 * Default feature flags (used when API is unavailable)
 */
export const defaultFeatureFlags: FeatureFlags = {
  checklist: true,
  chat: false,
  tasking: false,
  cobraKai: false,
  eventSummary: false,
  statusChart: false,
  eventTimeline: false,
  cobraAi: false,
};
