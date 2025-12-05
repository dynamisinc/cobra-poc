using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CobraAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeExternalChannelMappingEventIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalChannelMappings_Events_EventId",
                table: "ExternalChannelMappings");

            migrationBuilder.DropIndex(
                name: "IX_ExternalChannelMappings_EventId",
                table: "ExternalChannelMappings");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "ExternalChannelMappings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalChannelMappings_EventId",
                table: "ExternalChannelMappings",
                column: "EventId",
                filter: "[EventId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalChannelMappings_Events_EventId",
                table: "ExternalChannelMappings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExternalChannelMappings_Events_EventId",
                table: "ExternalChannelMappings");

            migrationBuilder.DropIndex(
                name: "IX_ExternalChannelMappings_EventId",
                table: "ExternalChannelMappings");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "ExternalChannelMappings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalChannelMappings_EventId",
                table: "ExternalChannelMappings",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalChannelMappings_Events_EventId",
                table: "ExternalChannelMappings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
