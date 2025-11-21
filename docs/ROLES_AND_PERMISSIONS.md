# Roles and Permissions - COBRA Checklist POC

> **Document Version:** 1.0.0
> **Last Updated:** 2025-11-21
> **Status:** Phase 1 Implementation

## Overview

The COBRA Checklist system implements a four-tier permission model aligned with C5 Design System standards. The model balances security with usability, ensuring emergency responders can access what they need without unnecessary friction.

### Design Philosophy

- **Mobile-first** - 70%+ of Contributors use mobile devices in the field
- **Minimal training** - Contributors: 30 minutes, Admins: 3-4 hours
- **Context-aware** - Smart defaults based on position and event category
- **Progressive disclosure** - Hide complexity from casual users

---

## Permission Levels

| Role | Access Level | Typical Users | Training Time |
|------|--------------|---------------|---------------|
| **None** | No access | Unauthorized users | N/A |
| **Readonly** | View only | Observers, auditors, external stakeholders | 15 minutes |
| **Contributor** | Create instances, interact with items | Field responders, section staff | **30 minutes** |
| **Manage** | Full administrative access | Incident commanders, system admins | 3-4 hours |

---

## Detailed Capabilities Matrix

### Feature Access by Role

| Feature | None | Readonly | Contributor | Manage |
|---------|------|----------|-------------|--------|
| **Navigation** |
| My Checklists (nav) | ❌ | ✅ | ✅ | ✅ |
| Template Library (nav) | ❌ | ❌ | ❌ | ✅ |
| Item Library (nav) | ❌ | ❌ | ❌ | ✅ |
| Analytics (nav) | ❌ | ❌ | ❌ | ✅ |
| **Templates** |
| View templates | ❌ | ❌ | ✅ (context only) | ✅ |
| Preview template | ❌ | ❌ | ✅ | ✅ |
| Create template | ❌ | ❌ | ❌ | ✅ |
| Edit template | ❌ | ❌ | ❌ | ✅ |
| Duplicate template | ❌ | ❌ | ❌ | ✅ |
| Archive template | ❌ | ❌ | ❌ | ✅ |
| View analytics | ❌ | ❌ | ❌ | ✅ |
| **Checklist Instances** |
| View own checklists | ❌ | ✅ | ✅ | ✅ |
| View all checklists | ❌ | ❌ | ❌ | ✅ |
| Create from template | ❌ | ❌ | ✅ | ✅ |
| Edit checklist metadata | ❌ | ❌ | ✅ (own) | ✅ |
| Archive checklist | ❌ | ❌ | ✅ (own) | ✅ |
| **Checklist Items** |
| View items | ❌ | ✅ | ✅ | ✅ |
| Toggle item completion | ❌ | ❌ | ✅ | ✅ |
| Change item status | ❌ | ❌ | ✅ | ✅ |
| Add item notes | ❌ | ❌ | ✅ | ✅ |
| Edit item text | ❌ | ❌ | ❌ | ✅ |
| Add/remove items | ❌ | ❌ | ❌ | ✅ |
| Reorder items | ❌ | ❌ | ❌ | ✅ |
| **Item Library** |
| View library | ❌ | ❌ | ❌ | ✅ |
| Create library item | ❌ | ❌ | ❌ | ✅ |
| Edit library item | ❌ | ❌ | ❌ | ✅ |
| Save template item to library | ❌ | ❌ | ❌ | ✅ |
| Add from library (template editor) | ❌ | ❌ | ❌ | ✅ |

---

## Role Details

### None - No Access

**Purpose:** Default state for unauthorized users or users not assigned to the tool.

**Access:**
- Cannot see the COBRA Checklist tool in C5
- Redirected to error page if attempting direct URL access

**Use Cases:**
- Users not involved in incident management
- Contractors without tool access
- Visitors to the C5 platform

---

### Readonly - View Only

**Purpose:** Observers who need visibility but should not modify data.

**Typical Users:**
- External auditors
- Legal/compliance reviewers
- Observers from other agencies
- Debrief participants (post-incident)

**Navigation:**
```
┌──────────────────────────────────┐
│  COBRA Checklist  [My Checklists]│  ← Only one nav item
└──────────────────────────────────┘
```

**What They See:**
- My Checklists page (filtered to checklists shared with them)
- Checklist detail pages (read-only)
- Item statuses and completion
- Item notes and history
- **Cannot** interact with checkboxes/dropdowns

**UI Indicators:**
- 🔒 Lock icon on all interactive elements
- Banner: "You have read-only access to this checklist"
- Tooltips: "Contact the checklist owner to request changes"

**Training Focus:**
- How to find assigned checklists
- Understanding checklist status
- Exporting/printing for documentation

---

### Contributor - Field Responder

**Purpose:** Front-line users who create and work on checklists during operations.

**Typical Users:**
- Safety Officers
- Operations Section Chiefs
- Planning Section Chiefs
- Logistics Coordinators
- Finance/Admin staff
- Strike Team Leaders
- Division/Group Supervisors

**Navigation (Mobile):**
```
┌─────────────────────────────────┐
│  COBRA Checklist   [Profile ▼] │
│                                  │
│  ┌────────┐ ┌────────┐         │
│  │   My   │ │ Create │         │
│  │Checks  │ │Checklist│         │
│  └────────┘ └────────┘         │
└─────────────────────────────────┘
```

**Navigation (Desktop):**
```
┌──────────────────────────────────────────────┐
│  COBRA Checklist  [My Checklists] [Profile ▼]│
└──────────────────────────────────────────────┘
```

**Primary Workflow:**
1. Open app → See active checklists
2. Tap "Create Checklist" → Get smart suggestions
3. Select template → Auto-fill context
4. Start working on checklist items

**Key Features:**
- ✅ **Smart template suggestions** based on:
  - Current position (e.g., Safety Officer)
  - Event category (e.g., Wildfire)
  - Recent templates used
  - Popular templates for position
- ✅ **One-tap checklist creation** with smart defaults
- ✅ **Mobile-optimized item interaction** (large touch targets)
- ✅ **Offline capability** (future) for field work
- ✅ **Real-time collaboration** via SignalR

**What They CAN Do:**
- Create new checklist instances from approved templates
- Complete checklist items (toggle checkboxes, change status)
- Add notes to items
- Mark items as N/A
- Archive completed checklists
- View item history and attribution

**What They CANNOT Do:**
- Create or edit templates (admin task)
- Access Item Library (admin tool)
- View system analytics
- Modify template-level settings
- Permanently delete checklists

**UI Experience:**
- **Simplified navigation** - Only My Checklists visible
- **Context-aware creation** - "Create Checklist" button prominent
- **Progressive disclosure** - Advanced options hidden by default
- **Immediate feedback** - Optimistic UI updates
- **Bottom sheets** (mobile) for template selection

**Training Focus:**
- Creating a checklist from template (5 minutes)
- Working on checklist items (10 minutes)
- Adding notes and context (5 minutes)
- Understanding real-time updates (5 minutes)
- Mobile gestures (swipe actions) (5 minutes)

---

### Manage - System Administrator

**Purpose:** Full control over templates, analytics, and system configuration.

**Typical Users:**
- Incident Commanders
- System administrators
- Planning Chiefs (template management)
- Training coordinators

**Navigation (Full Access):**
```
┌────────────────────────────────────────────────────────────┐
│  COBRA  [My Checklists] [Template Library] [Item Library]  │
│         [Profile ▼]                                        │
└────────────────────────────────────────────────────────────┘
```

**Key Capabilities:**

#### Template Management
- Create templates from scratch or duplicate existing
- Full template editor with drag-drop reordering
- Item Library integration (save/reuse common items)
- Template validation warnings
- Archive/restore templates
- View template usage analytics

#### Item Library
- Create reusable checklist items
- Organize by category
- Track usage counts
- Add to templates with one click
- Edit library items globally

#### Analytics & Insights
- Dashboard with system metrics
- Most/least used templates
- Popular library items
- Usage trends over time
- Template effectiveness scoring

#### System Configuration
- Manage template categories
- Set default templates for positions
- Configure event categories
- Manage operational periods

**Training Focus:**
- Template design best practices (1 hour)
- Using the visual item builder (30 minutes)
- Item Library workflows (30 minutes)
- Analytics interpretation (30 minutes)
- Advanced features (collapse/expand, position restrictions) (30 minutes)
- System administration (30 minutes)

---

## Template Discovery - Contributor vs Manage

### Contributor: Context-Driven Discovery

**Goal:** Get to work quickly with minimal navigation.

**Flow:**
```
My Checklists → Tap "Create Checklist"
    ↓
Smart Suggestions (Bottom Sheet)
    ├── 📋 Suggested for Your Position (3 templates)
    ├── 🔥 Fire Response Templates (2 templates)
    ├── 🕐 Recently Used (2 templates)
    └── 🔍 Search All Templates (escape hatch)
    ↓
Template Preview (inline)
    ↓
Quick Instance Creation (auto-filled)
    ↓
Working Checklist
```

**Design Principles:**
- **3 taps from home to working checklist**
- Zero typing required (smart defaults)
- Search available but not required
- Preview embedded in flow

### Manage: Full Library Access

**Goal:** Browse, analyze, and manage all templates.

**Flow:**
```
Template Library (dedicated page)
    ├── Analytics Dashboard (collapsible)
    ├── Search & Filters
    ├── Category Groups
    ├── Template Cards
    │   ├── Preview
    │   ├── Edit
    │   ├── Duplicate
    │   └── Analytics
    └── Create New Template
```

**Design Principles:**
- Full visibility into all templates
- Analytics for decision-making
- Bulk operations (future)
- Admin-specific actions visible

---

## Smart Template Suggestions

### Ranking Algorithm

Templates are suggested to Contributors based on weighted scoring:

```
Score = (PositionMatch × 10) + (EventCategoryMatch × 8) +
        (RecentlyUsed × 6) + (PopularityScore × 4) +
        (SeasonalRelevance × 2)
```

**Factors:**

1. **Position Match (Weight: 10)** - Highest priority
   - Template metadata: `recommendedPositions: ["Safety Officer", "Ops Chief"]`
   - Dynamic tracking: "Safety Officers use this 85% of the time"

2. **Event Category Match (Weight: 8)**
   - Primary category: Fire, Flood, Medical, Concert, etc.
   - Additional categories: Wildfire, Structure Fire, Urban Fire
   - Template metadata: `eventCategories: ["Fire", "Wildfire"]`

3. **Recently Used (Weight: 6)**
   - User's last 5 templates
   - Time decay: Last 24h = full weight, 7 days = 50% weight

4. **Popularity Score (Weight: 4)**
   - Usage count across all users
   - Normalized by template age

5. **Seasonal Relevance (Weight: 2)** - Future enhancement
   - Hurricane season templates boosted Jun-Nov
   - Winter storm templates boosted Dec-Mar

### Example Suggestion Output

For **Safety Officer** during **Wildfire Event**:

```json
{
  "suggestions": [
    {
      "templateId": "abc-123",
      "name": "Daily Safety Briefing",
      "score": 94,
      "reasons": [
        "Most used by Safety Officers (45 times)",
        "Recommended for Wildfire events",
        "You used this yesterday"
      ],
      "itemCount": 12,
      "avgCompletionTime": "15 minutes"
    },
    {
      "templateId": "def-456",
      "name": "PPE Inspection Checklist",
      "score": 88,
      "reasons": [
        "Recommended for Safety Officers",
        "Popular for Fire events (38 uses)"
      ],
      "itemCount": 8,
      "avgCompletionTime": "10 minutes"
    }
  ]
}
```

---

## Template Metadata for Smart Suggestions

### Recommended Fields (Phase 2)

Add to `Template` entity:

```csharp
public class Template
{
    // ... existing fields ...

    /// <summary>
    /// Positions this template is recommended for
    /// JSON array: ["Safety Officer", "Operations Section Chief"]
    /// </summary>
    public string? RecommendedPositions { get; set; }

    /// <summary>
    /// Event categories this template applies to
    /// JSON array: ["Fire", "Wildfire", "Structure Fire"]
    /// </summary>
    public string? EventCategories { get; set; }

    /// <summary>
    /// Estimated completion time in minutes
    /// Used for user planning
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Usage frequency score (auto-calculated)
    /// Updated nightly based on actual usage
    /// </summary>
    public int UsageScore { get; set; } = 0;

    /// <summary>
    /// Last time this template was used (auto-tracked)
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}
```

### Seeding Strategy

**Initial Seed (Manual Curation):**
```sql
-- Example: Daily Safety Briefing
UPDATE Templates
SET RecommendedPositions = '["Safety Officer", "Incident Commander"]',
    EventCategories = '["Fire", "Wildfire", "Medical", "All-Hazard"]',
    EstimatedMinutes = 15,
    UsageScore = 50  -- Start with moderate score
WHERE Name = 'Daily Safety Briefing';
```

**Dynamic Updates (Automated):**
- Nightly job calculates `UsageScore` based on actual usage
- Tracks which positions use which templates
- Adjusts `RecommendedPositions` if usage patterns change
- Updates `LastUsedAt` on every instance creation

---

## Event Categories

### Primary Categories (Required)

| Category | Subcategories | Typical Templates |
|----------|---------------|-------------------|
| **Fire** | Wildfire, Structure Fire, Vehicle Fire | Safety Briefing, Resource Tracking, ICS Forms |
| **Flood** | River Flood, Flash Flood, Urban Flood | Evacuation, Shelter Operations |
| **Medical** | Mass Casualty, Pandemic, Biohazard | Triage, PPE, Decontamination |
| **Weather** | Hurricane, Tornado, Winter Storm | Shelter Inspection, Damage Assessment |
| **Hazmat** | Chemical, Biological, Radiological | Entry Team Briefing, Decon Setup |
| **Security** | Active Shooter, Bomb Threat, Protest | Lockdown Procedures, Perimeter Security |
| **Special Event** | Concert, Sporting Event, Parade | Crowd Control, First Aid Stations |
| **All-Hazard** | Generic, Multi-Use | Daily Safety, ICS-214, Communications |

### Additional Categories (Optional)

Templates can have multiple categories for better discoverability:
- **Primary:** "Fire"
- **Additional:** ["Wildfire", "Structure Fire", "Urban Interface"]

This allows fine-tuned suggestions while keeping UI simple.

---

## User Experience Scenarios

### Scenario 1: First-Time Contributor

**User:** Maria, Safety Officer, just assigned to a wildfire

**Experience:**
1. Opens app on mobile → Sees empty "My Checklists"
2. Tap "Create Checklist" button
3. Bottom sheet shows: "Suggested for Safety Officer"
4. Sees "Daily Safety Briefing" (most used by Safety Officers)
5. Taps "Use Template" → Auto-fills: Event, Op Period, Position
6. Taps "Create" → Immediately starts working on checklist
7. **Total time:** 30 seconds

**Training needed:** 5 minutes (shown the button, told to tap it)

### Scenario 2: Experienced Contributor

**User:** John, Operations Chief, has used app many times

**Experience:**
1. Opens app → Sees 3 active checklists
2. Tap "Create Checklist" → Suggestions include "Recently Used"
3. Sees template used yesterday → One tap to reuse
4. Modifies checklist name → Creates
5. **Total time:** 15 seconds

**Training needed:** Already knows the flow

### Scenario 3: Readonly Observer

**User:** Inspector from regulatory agency

**Experience:**
1. Opens app → Sees checklists shared with them
2. Taps checklist → Opens detail view
3. Sees 🔒 lock icon and "Read-only access" banner
4. Can scroll through items, see completion status
5. Can view notes and history
6. **Cannot** toggle checkboxes (grayed out)

**Training needed:** 10 minutes (how to navigate, export)

### Scenario 4: Admin Creating Template

**User:** Planning Chief setting up templates for hurricane season

**Experience:**
1. Opens Template Library
2. Clicks "Create New Template"
3. Uses visual editor with drag-drop
4. Adds items from Item Library
5. Sets metadata:
   - Category: "Weather"
   - Event categories: ["Hurricane", "Tropical Storm"]
   - Recommended positions: ["Logistics Chief", "Shelter Manager"]
   - Estimated time: 20 minutes
6. Preview → Validates → Saves
7. Template immediately available to Contributors

**Training needed:** 2 hours (template design, best practices)

---

## Technical Implementation Notes

### Backend: Permission Middleware

```csharp
// MockUserMiddleware enhancement
public class MockUser
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Position { get; set; }
    public List<string> Positions { get; set; }  // New: Multi-position support
    public string PermissionRole { get; set; }   // New: "None", "Readonly", "Contributor", "Manage"
    public string EventId { get; set; }
    public string EventName { get; set; }
    public string EventCategory { get; set; }    // New: "Fire", "Flood", etc.
}
```

### Frontend: Role-Based Rendering

```typescript
// Hook for permission checks
export const usePermissions = () => {
  const role = mockUser.permissionRole; // From context

  return {
    canCreateTemplate: role === 'Manage',
    canEditTemplate: role === 'Manage',
    canAccessItemLibrary: role === 'Manage',
    canViewAnalytics: role === 'Manage',
    canCreateInstance: ['Contributor', 'Manage'].includes(role),
    canInteractWithItems: ['Contributor', 'Manage'].includes(role),
    canViewChecklists: ['Readonly', 'Contributor', 'Manage'].includes(role),
  };
};
```

### Navigation Filtering

```typescript
const NavBar = () => {
  const { canCreateTemplate, canAccessItemLibrary } = usePermissions();

  return (
    <AppBar>
      <Button to="/checklists">My Checklists</Button>
      {canCreateTemplate && <Button to="/templates">Template Library</Button>}
      {canAccessItemLibrary && <Button to="/item-library">Item Library</Button>}
    </AppBar>
  );
};
```

---

## Migration Path: Current → Phase 1

### Current State (No Permissions)
- All features visible to all users
- No role differentiation
- Position selector only

### Phase 1 (Permission Gating)
- Profile menu with position(s) + role selection
- Navigation filtered by role
- "Create Checklist" button on My Checklists
- Basic template picker dialog

### Phase 2 (Smart Suggestions)
- Template metadata (positions, categories)
- Suggestion algorithm
- Enhanced template picker with suggestions
- Recently used tracking

### Phase 3 (Mobile Polish)
- Bottom sheets for mobile
- Swipe actions
- Touch-optimized UI
- Offline support

---

## Open Questions

1. **Event Category Assignment:**
   - Does user set event category when creating event?
   - Or is it inferred from event type/name?
   - Should it be editable during event?

2. **Multi-Position Workflows:**
   - Can a user have multiple positions simultaneously?
   - Example: Safety Officer + Operations Section Chief
   - Should suggestions merge results from all positions?

3. **Template Visibility:**
   - Should Contributors see ALL templates or only relevant ones?
   - Filter by position automatically?
   - Filter by event category automatically?

4. **Permission Inheritance:**
   - If user is Manage at org level, are they Manage at event level?
   - Can permissions be scoped to specific events?

5. **Readonly Sharing:**
   - How do Readonly users get access to checklists?
   - Explicit sharing by checklist owner?
   - Automatic based on event participation?

---

## Success Metrics

### Contributor Efficiency (30-minute training goal)
- [ ] 90% can create checklist in < 1 minute (after 10-minute training)
- [ ] 95% can complete items without assistance
- [ ] 80% prefer mobile app over desktop
- [ ] < 5% request features from Template Library

### Admin Effectiveness (3-4 hour training goal)
- [ ] Can create basic template in < 15 minutes
- [ ] Can use Item Library effectively
- [ ] Understand analytics dashboard
- [ ] Can troubleshoot common user issues

### System Adoption
- [ ] 70%+ of instances created via smart suggestions (not manual search)
- [ ] 60%+ of templates used at least once per month
- [ ] < 10% of templates unused after 90 days

---

## Future Enhancements

### Phase 4: Advanced Suggestions
- Machine learning for template recommendations
- "Users like you also used..." suggestions
- Predictive next-template recommendations

### Phase 5: Collaboration Features
- Template comments/feedback from Contributors
- "Request new template" workflow
- Template ratings/reviews

### Phase 6: Organizational Features
- Template approval workflows
- Version control for templates
- Template inheritance (org → customer → event)

---

## References

- [UI_PATTERNS.md](./UI_PATTERNS.md) - UX design guidelines
- [USER-STORIES.md](./USER-STORIES.md) - User requirements
- [CLAUDE.md](../CLAUDE.md) - Development guide
- C5 Design System - Permission standards
