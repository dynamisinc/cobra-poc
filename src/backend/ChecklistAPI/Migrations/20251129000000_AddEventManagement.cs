using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChecklistAPI.Migrations
{
    /// <summary>
    /// Adds Event and EventCategory tables with FEMA/NIMS standard categories.
    ///
    /// BREAKING CHANGE: This migration changes EventId columns from string to GUID.
    /// Existing data in ChecklistInstances and OperationalPeriods will need to be migrated.
    /// For POC: Consider dropping and recreating the database.
    /// </summary>
    public partial class AddEventManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create EventCategories table
            migrationBuilder.CreateTable(
                name: "EventCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_Code",
                table: "EventCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_EventType",
                table: "EventCategories",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_EventType_DisplayOrder",
                table: "EventCategories",
                columns: new[] { "EventType", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_IsActive",
                table: "EventCategories",
                column: "IsActive");

            // Step 2: Seed FEMA/NIMS standard categories
            SeedEventCategories(migrationBuilder);

            // Step 3: Create Events table
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrimaryCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalCategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_EventCategories_PrimaryCategoryId",
                        column: x => x.PrimaryCategoryId,
                        principalTable: "EventCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType",
                table: "Events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsActive",
                table: "Events",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsArchived",
                table: "Events",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PrimaryCategoryId",
                table: "Events",
                column: "PrimaryCategoryId");

            // Step 4: Seed a default event for POC
            SeedDefaultEvent(migrationBuilder);

            // Step 5: Modify ChecklistInstances - drop old column, add new one
            // Drop the old index first
            migrationBuilder.DropIndex(
                name: "IX_ChecklistInstances_EventId",
                table: "ChecklistInstances");

            // Drop old EventId column (string)
            migrationBuilder.DropColumn(
                name: "EventId",
                table: "ChecklistInstances");

            // Add new EventId column (GUID)
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "ChecklistInstances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001")); // Will be updated to default event

            // Update EventName max length
            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "ChecklistInstances",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Add FK constraint and index
            migrationBuilder.CreateIndex(
                name: "IX_ChecklistInstances_EventId",
                table: "ChecklistInstances",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistInstances_Events_EventId",
                table: "ChecklistInstances",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Step 6: Modify OperationalPeriods - drop old column, add new one
            // Drop the old indexes first
            migrationBuilder.DropIndex(
                name: "IX_OperationalPeriods_EventId",
                table: "OperationalPeriods");

            migrationBuilder.DropIndex(
                name: "IX_OperationalPeriods_EventId_IsCurrent",
                table: "OperationalPeriods");

            // Drop old EventId column (string)
            migrationBuilder.DropColumn(
                name: "EventId",
                table: "OperationalPeriods");

            // Add new EventId column (GUID)
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "OperationalPeriods",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001")); // Will be updated to default event

            // Add FK constraint and indexes
            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId",
                table: "OperationalPeriods",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId_IsCurrent",
                table: "OperationalPeriods",
                columns: new[] { "EventId", "IsCurrent" });

            migrationBuilder.AddForeignKey(
                name: "FK_OperationalPeriods_Events_EventId",
                table: "OperationalPeriods",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove FK from OperationalPeriods
            migrationBuilder.DropForeignKey(
                name: "FK_OperationalPeriods_Events_EventId",
                table: "OperationalPeriods");

            migrationBuilder.DropIndex(
                name: "IX_OperationalPeriods_EventId",
                table: "OperationalPeriods");

            migrationBuilder.DropIndex(
                name: "IX_OperationalPeriods_EventId_IsCurrent",
                table: "OperationalPeriods");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "OperationalPeriods");

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "OperationalPeriods",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId",
                table: "OperationalPeriods",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId_IsCurrent",
                table: "OperationalPeriods",
                columns: new[] { "EventId", "IsCurrent" });

            // Remove FK from ChecklistInstances
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistInstances_Events_EventId",
                table: "ChecklistInstances");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistInstances_EventId",
                table: "ChecklistInstances");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "ChecklistInstances");

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "ChecklistInstances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EventName",
                table: "ChecklistInstances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistInstances_EventId",
                table: "ChecklistInstances",
                column: "EventId");

            // Drop Events table
            migrationBuilder.DropTable(name: "Events");

            // Drop EventCategories table
            migrationBuilder.DropTable(name: "EventCategories");
        }

        /// <summary>
        /// Seeds FEMA/NIMS standard event categories
        /// </summary>
        private void SeedEventCategories(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // PLANNED EVENT CATEGORIES
            // ============================================================
            var plannedCategories = new[]
            {
                ("PARADE", "Parade/Procession", "Special Event", 1, "flag"),
                ("AIRSHOW", "Air Show", "Special Event", 2, "plane"),
                ("TRAINING", "Training Exercise", "Special Event", 3, "graduation-cap"),
                ("FESTIVAL", "Festival/Fair", "Special Event", 4, "tent"),
                ("CONCERT", "Concert/Performance", "Special Event", 5, "music"),
                ("SPORTING", "Sporting Event", "Special Event", 6, "trophy"),
                ("VIP_VISIT", "VIP Visit/Dignitary", "Special Event", 7, "user-tie"),
                ("COMMUNITY", "Community Event", "Special Event", 8, "users"),
                ("CONFERENCE", "Conference/Convention", "Special Event", 9, "building"),
                ("OTHER_PLANNED", "Other Planned Event", "Special Event", 99, "calendar"),
            };

            int order = 1;
            foreach (var (code, name, subGroup, displayOrder, icon) in plannedCategories)
            {
                migrationBuilder.InsertData(
                    table: "EventCategories",
                    columns: new[] { "Id", "Code", "Name", "EventType", "SubGroup", "DisplayOrder", "IsActive", "IconName" },
                    values: new object[] { Guid.NewGuid(), code, name, "PLANNED", subGroup, order++, true, icon });
            }

            // ============================================================
            // UNPLANNED EVENT CATEGORIES - Natural Disasters (FEMA NRI 18)
            // ============================================================
            var naturalDisasters = new[]
            {
                ("HURRICANE", "Hurricane/Tropical Storm", "hurricane"),
                ("TORNADO", "Tornado", "wind"),
                ("FLOOD", "Flood (Coastal/Riverine)", "water"),
                ("WILDFIRE", "Wildfire", "fire"),
                ("EARTHQUAKE", "Earthquake", "house-crack"),
                ("WINTER_STORM", "Winter Storm/Ice Storm", "snowflake"),
                ("SEVERE_STORM", "Severe Thunderstorm/Hail", "cloud-bolt"),
                ("DROUGHT", "Drought", "sun"),
                ("TSUNAMI", "Tsunami", "water"),
                ("VOLCANIC", "Volcanic Activity", "mountain"),
                ("LANDSLIDE", "Landslide/Avalanche", "hill-rockslide"),
                ("HEAT_WAVE", "Heat Wave", "temperature-high"),
                ("COLD_WAVE", "Cold Wave", "temperature-low"),
                ("LIGHTNING", "Lightning", "bolt"),
            };

            order = 1;
            foreach (var (code, name, icon) in naturalDisasters)
            {
                migrationBuilder.InsertData(
                    table: "EventCategories",
                    columns: new[] { "Id", "Code", "Name", "EventType", "SubGroup", "DisplayOrder", "IsActive", "IconName" },
                    values: new object[] { Guid.NewGuid(), code, name, "UNPLANNED", "Natural Disaster", order++, true, icon });
            }

            // ============================================================
            // UNPLANNED EVENT CATEGORIES - Technological/Human-Caused
            // ============================================================
            var technologicalHazards = new[]
            {
                ("HAZMAT", "Hazmat Incident", "biohazard"),
                ("TRANSPORT_AIR", "Transportation - Aviation", "plane-crash"),
                ("TRANSPORT_RAIL", "Transportation - Rail", "train"),
                ("TRANSPORT_MARINE", "Transportation - Marine", "ship"),
                ("TRANSPORT_VEHICLE", "Transportation - Vehicle/MCI", "car-burst"),
                ("STRUCTURE_FIRE", "Structure Fire", "fire-flame-curved"),
                ("INFRASTRUCTURE", "Infrastructure Failure", "plug-circle-exclamation"),
                ("POWER_OUTAGE", "Power Outage", "power-off"),
                ("CYBER", "Cyber Incident", "shield-virus"),
                ("TERRORISM", "Terrorism/Intentional Act", "skull"),
                ("ACTIVE_SHOOTER", "Active Shooter/Threat", "person-rifle"),
                ("CIVIL_UNREST", "Civil Disturbance", "people-group"),
                ("MASS_CASUALTY", "Mass Casualty Incident", "hospital"),
                ("SAR", "Search and Rescue", "life-ring"),
                ("PUBLIC_HEALTH", "Public Health Emergency", "virus"),
                ("OTHER_EMERGENCY", "Other Emergency", "triangle-exclamation"),
            };

            order = 100; // Start after natural disasters
            foreach (var (code, name, icon) in technologicalHazards)
            {
                migrationBuilder.InsertData(
                    table: "EventCategories",
                    columns: new[] { "Id", "Code", "Name", "EventType", "SubGroup", "DisplayOrder", "IsActive", "IconName" },
                    values: new object[] { Guid.NewGuid(), code, name, "UNPLANNED", "Technological/Human-Caused", order++, true, icon });
            }
        }

        /// <summary>
        /// Seeds a default event for POC testing
        /// </summary>
        private void SeedDefaultEvent(MigrationBuilder migrationBuilder)
        {
            // First, get the Hurricane category ID (we need a known ID for the default)
            // Using a fixed GUID for the default event
            var defaultEventId = new Guid("00000000-0000-0000-0000-000000000001");

            // Insert a placeholder - the actual category ID will need to be looked up
            // For now, we'll use raw SQL to handle this properly
            migrationBuilder.Sql(@"
                DECLARE @HurricaneCategoryId uniqueidentifier;
                SELECT TOP 1 @HurricaneCategoryId = Id FROM EventCategories WHERE Code = 'HURRICANE';

                INSERT INTO Events (Id, Name, EventType, PrimaryCategoryId, AdditionalCategoryIds, IsActive, IsArchived, CreatedBy, CreatedAt)
                VALUES ('00000000-0000-0000-0000-000000000001', 'Hurricane Milton Response', 'UNPLANNED', @HurricaneCategoryId, NULL, 1, 0, 'System', GETUTCDATE());
            ");
        }
    }
}
