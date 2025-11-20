-- Seed Data for Item Library
-- Provides pre-populated library items for common emergency management tasks
-- Categories: Safety, Operations, Planning, Logistics, Communications, Documentation, Medical

-- Clear existing seed data (optional - comment out if you want to preserve existing items)
-- DELETE FROM ItemLibraryEntries WHERE CreatedBy = 'System Seed';

-- Safety Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Verify all personnel have appropriate safety equipment (PPE, helmets, vests)', 'checkbox', 'Safety', NULL, NULL, 'Check hard hats, safety vests, gloves, and eye protection', '["safety", "ppe", "daily", "equipment"]', 1, 45, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Complete safety briefing with all operational personnel', 'checkbox', 'Safety', NULL, '["Incident Commander", "Operations Section Chief", "Safety Officer"]', 'Include hazards, escape routes, and emergency procedures', '["safety", "briefing", "daily"]', 1, 38, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Conduct safety hazard assessment for operational area', 'status', 'Safety', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"In Progress","isCompletion":false,"order":2},{"label":"Complete","isCompletion":true,"order":3},{"label":"Needs Review","isCompletion":false,"order":4}]', '["Safety Officer"]', NULL, '["safety", "assessment", "hazard"]', 1, 23, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Check and test all emergency communication equipment', 'checkbox', 'Safety', NULL, NULL, 'Radios, satellite phones, emergency beacons', '["safety", "communications", "equipment"]', 1, 31, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Establish and mark safe zones and evacuation routes', 'checkbox', 'Safety', NULL, '["Incident Commander", "Safety Officer", "Operations Section Chief"]', 'Post signs and brief all personnel', '["safety", "evacuation", "setup"]', 1, 19, 'System Seed', GETUTCDATE(), 0);

-- Operations Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Review and approve Incident Action Plan (IAP)', 'checkbox', 'Operations', NULL, '["Incident Commander", "Operations Section Chief", "Planning Section Chief"]', 'Ensure all sections reviewed and signed off', '["operations", "iap", "planning", "daily"]', 1, 42, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Conduct operational period briefing', 'checkbox', 'Operations', NULL, '["Incident Commander", "Operations Section Chief"]', 'Brief all division and unit leaders', '["operations", "briefing", "daily"]', 1, 35, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Assign resources to operational divisions and groups', 'status', 'Operations', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"In Progress","isCompletion":false,"order":2},{"label":"Complete","isCompletion":true,"order":3}]', '["Operations Section Chief"]', NULL, '["operations", "resources", "assignment"]', 1, 28, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Verify span of control ratios (1:5 to 1:7)', 'checkbox', 'Operations', NULL, '["Incident Commander", "Operations Section Chief"]', 'Check supervisory ratios across all divisions', '["operations", "management", "supervision"]', 0, 15, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Conduct end-of-operational-period debriefing', 'checkbox', 'Operations', NULL, '["Operations Section Chief"]', 'Document lessons learned and issues', '["operations", "debriefing", "lessons-learned"]', 1, 27, 'System Seed', GETUTCDATE(), 0);

-- Planning Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Review weather forecast and environmental conditions', 'checkbox', 'Planning', NULL, '["Planning Section Chief", "Operations Section Chief"]', 'Check NWS forecasts and local conditions', '["planning", "weather", "daily"]', 1, 33, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Update situation status and incident objectives', 'status', 'Planning', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"In Progress","isCompletion":false,"order":2},{"label":"Complete","isCompletion":true,"order":3}]', '["Planning Section Chief", "Incident Commander"]', NULL, '["planning", "objectives", "situation"]', 1, 29, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Prepare Incident Action Plan (IAP) for next operational period', 'status', 'Planning', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"Drafting","isCompletion":false,"order":2},{"label":"Review","isCompletion":false,"order":3},{"label":"Approved","isCompletion":true,"order":4}]', '["Planning Section Chief"]', NULL, '["planning", "iap", "documentation"]', 1, 37, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Review maps and geographic information for operational area', 'checkbox', 'Planning', NULL, '["Planning Section Chief"]', 'Update maps with current conditions and changes', '["planning", "maps", "gis"]', 0, 18, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Conduct planning meeting with all section chiefs', 'checkbox', 'Planning', NULL, '["Planning Section Chief", "Incident Commander"]', 'Review objectives, tactics, and resource needs', '["planning", "meeting", "coordination"]', 1, 31, 'System Seed', GETUTCDATE(), 0);

-- Logistics Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Check fuel levels for all vehicles and generators', 'checkbox', 'Logistics', NULL, '["Logistics Section Chief"]', 'Arrange refueling if below 50%', '["logistics", "fuel", "equipment", "daily"]', 1, 26, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Verify food and water supplies for all personnel', 'checkbox', 'Logistics', NULL, '["Logistics Section Chief"]', 'Order additional supplies if needed for next period', '["logistics", "supplies", "food", "daily"]', 1, 24, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Inspect and maintain communications equipment', 'status', 'Logistics', '[{"label":"Not Checked","isCompletion":false,"order":1},{"label":"Inspecting","isCompletion":false,"order":2},{"label":"Operational","isCompletion":true,"order":3},{"label":"Needs Repair","isCompletion":false,"order":4}]', '["Logistics Section Chief"]', NULL, '["logistics", "communications", "equipment", "maintenance"]', 1, 22, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Arrange transportation for personnel and equipment', 'checkbox', 'Logistics', NULL, '["Logistics Section Chief"]', 'Coordinate vehicles, drivers, and schedules', '["logistics", "transportation"]', 0, 17, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Order and receive equipment and supplies for next operational period', 'status', 'Logistics', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"Ordered","isCompletion":false,"order":2},{"label":"Received","isCompletion":true,"order":3}]', '["Logistics Section Chief"]', NULL, '["logistics", "supplies", "ordering"]', 1, 20, 'System Seed', GETUTCDATE(), 0);

-- Communications Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Test all radio channels and frequencies', 'checkbox', 'Communications', NULL, '["Communications Unit Leader", "Incident Commander"]', 'Verify primary and backup channels', '["communications", "radio", "equipment", "daily"]', 1, 30, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Establish communications plan and distribute to all units', 'checkbox', 'Communications', NULL, '["Communications Unit Leader", "Planning Section Chief"]', 'Include radio frequencies, phone numbers, and protocols', '["communications", "planning", "coordination"]', 1, 25, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Verify contact information for all key personnel', 'checkbox', 'Communications', NULL, '["Communications Unit Leader"]', 'Maintain updated contact roster', '["communications", "contacts", "personnel"]', 1, 21, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Configure and test satellite or backup communication systems', 'status', 'Communications', '[{"label":"Not Configured","isCompletion":false,"order":1},{"label":"Configured","isCompletion":false,"order":2},{"label":"Tested","isCompletion":true,"order":3}]', '["Communications Unit Leader"]', NULL, '["communications", "satellite", "backup", "equipment"]', 0, 12, 'System Seed', GETUTCDATE(), 0);

-- Documentation Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Complete ICS-214 Activity Log for operational period', 'checkbox', 'Documentation', NULL, NULL, 'Document all significant events and actions', '["documentation", "ics-214", "daily", "required"]', 1, 40, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Update ICS-201 Incident Briefing', 'status', 'Documentation', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"Drafting","isCompletion":false,"order":2},{"label":"Complete","isCompletion":true,"order":3}]', '["Incident Commander", "Planning Section Chief"]', NULL, '["documentation", "ics-201", "briefing"]', 1, 32, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Complete ICS-209 Incident Status Summary', 'status', 'Documentation', '[{"label":"Not Started","isCompletion":false,"order":1},{"label":"In Progress","isCompletion":false,"order":2},{"label":"Complete","isCompletion":true,"order":3},{"label":"Submitted","isCompletion":true,"order":4}]', '["Planning Section Chief"]', 'Submit to higher authority as required', '["documentation", "ics-209", "reporting"]', 1, 28, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Document resource assignments (ICS-204)', 'checkbox', 'Documentation', NULL, '["Operations Section Chief", "Planning Section Chief"]', 'Complete for each division/group', '["documentation", "ics-204", "resources"]', 1, 24, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Take and archive photos/videos of incident area', 'checkbox', 'Documentation', NULL, '["Planning Section Chief", "Documentation Unit Leader"]', 'Date-stamp and catalog all media', '["documentation", "photos", "evidence"]', 0, 16, 'System Seed', GETUTCDATE(), 0);

-- Medical Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Verify medical supplies and first aid kits are stocked', 'checkbox', 'Medical', NULL, '["Medical Unit Leader", "Safety Officer"]', 'Check expiration dates on medications', '["medical", "supplies", "first-aid", "daily"]', 1, 19, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Establish medical aid station location and notify all personnel', 'checkbox', 'Medical', NULL, '["Medical Unit Leader", "Incident Commander"]', 'Post clear signage and mark on maps', '["medical", "aid-station", "setup"]', 1, 14, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Review emergency medical transport plan', 'checkbox', 'Medical', NULL, '["Medical Unit Leader", "Logistics Section Chief"]', 'Verify helicopter LZ or ambulance access', '["medical", "transport", "emergency", "planning"]', 1, 13, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Document any personnel injuries or medical incidents', 'status', 'Medical', '[{"label":"No Incidents","isCompletion":true,"order":1},{"label":"Minor Injury","isCompletion":false,"order":2},{"label":"Major Injury","isCompletion":false,"order":3},{"label":"Documented","isCompletion":true,"order":4}]', '["Medical Unit Leader", "Safety Officer"]', NULL, '["medical", "injuries", "documentation", "daily"]', 1, 11, 'System Seed', GETUTCDATE(), 0);

-- Equipment Items
INSERT INTO ItemLibraryEntries (Id, ItemText, ItemType, Category, StatusConfiguration, AllowedPositions, DefaultNotes, Tags, IsRequiredByDefault, UsageCount, CreatedBy, CreatedAt, IsArchived)
VALUES
(NEWID(), 'Conduct equipment inventory and accountability check', 'checkbox', 'Equipment', NULL, '["Logistics Section Chief"]', 'Verify all issued equipment is accounted for', '["equipment", "inventory", "accountability"]', 1, 18, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Inspect and test generators and power equipment', 'status', 'Equipment', '[{"label":"Not Checked","isCompletion":false,"order":1},{"label":"Inspected","isCompletion":false,"order":2},{"label":"Operational","isCompletion":true,"order":3},{"label":"Needs Repair","isCompletion":false,"order":4}]', '["Logistics Section Chief"]', NULL, '["equipment", "generators", "power", "maintenance"]', 1, 15, 'System Seed', GETUTCDATE(), 0),
(NEWID(), 'Check and service vehicles (maintenance, fluids, tires)', 'checkbox', 'Equipment', NULL, '["Logistics Section Chief"]', 'Perform daily vehicle inspections', '["equipment", "vehicles", "maintenance", "daily"]', 1, 17, 'System Seed', GETUTCDATE(), 0);

-- Print seed data summary
PRINT '✅ Item Library seeded successfully!';
PRINT 'Total items: 40+ across 8 categories';
PRINT 'Categories: Safety, Operations, Planning, Logistics, Communications, Documentation, Medical, Equipment';
PRINT 'Usage counts simulate realistic popularity (11-45 uses)';
PRINT '';
PRINT 'Most popular items:';
PRINT '  - Verify safety equipment (45 uses)';
PRINT '  - Review IAP (42 uses)';
PRINT '  - Complete ICS-214 Activity Log (40 uses)';
