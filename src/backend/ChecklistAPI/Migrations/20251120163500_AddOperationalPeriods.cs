using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChecklistAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop old OperationalPeriodId column (was string, needs to be Guid)
            migrationBuilder.DropColumn(
                name: "OperationalPeriodId",
                table: "ChecklistInstances");

            // 2. Create OperationalPeriods table
            migrationBuilder.CreateTable(
                name: "OperationalPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ArchivedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalPeriods", x => x.Id);
                });

            // 3. Create indexes on OperationalPeriods
            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId",
                table: "OperationalPeriods",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_EventId_IsCurrent",
                table: "OperationalPeriods",
                columns: new[] { "EventId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalPeriods_IsArchived",
                table: "OperationalPeriods",
                column: "IsArchived");

            // 4. Add new OperationalPeriodId column as Guid? FK to ChecklistInstances
            migrationBuilder.AddColumn<Guid>(
                name: "OperationalPeriodId",
                table: "ChecklistInstances",
                type: "uniqueidentifier",
                nullable: true);

            // 5. Create FK constraint with ON DELETE SET NULL
            migrationBuilder.CreateIndex(
                name: "IX_ChecklistInstances_OperationalPeriodId",
                table: "ChecklistInstances",
                column: "OperationalPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistInstances_OperationalPeriods_OperationalPeriodId",
                table: "ChecklistInstances",
                column: "OperationalPeriodId",
                principalTable: "OperationalPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop FK constraint
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistInstances_OperationalPeriods_OperationalPeriodId",
                table: "ChecklistInstances");

            // 2. Drop index on ChecklistInstances
            migrationBuilder.DropIndex(
                name: "IX_ChecklistInstances_OperationalPeriodId",
                table: "ChecklistInstances");

            // 3. Drop OperationalPeriodId column (Guid)
            migrationBuilder.DropColumn(
                name: "OperationalPeriodId",
                table: "ChecklistInstances");

            // 4. Drop OperationalPeriods table
            migrationBuilder.DropTable(
                name: "OperationalPeriods");

            // 5. Re-add OperationalPeriodId as string (previous schema)
            migrationBuilder.AddColumn<string>(
                name: "OperationalPeriodId",
                table: "ChecklistInstances",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
