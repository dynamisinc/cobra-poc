using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CobraAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedChatChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ChatThreads",
                columns: new[] { "Id", "ChannelType", "Color", "CreatedAt", "CreatedBy", "Description", "DisplayOrder", "EventId", "ExternalChannelMappingId", "IconName", "IsActive", "IsDefaultEventThread", "LastModifiedAt", "LastModifiedBy", "Name", "PositionId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0001-0001-0001-000000000001"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "General event discussion for all participants", 0, new Guid("00000000-0000-0000-0000-000000000001"), null, "comments", true, true, null, null, "Event Chat", null },
                    { new Guid("aaaaaaaa-0001-0001-0001-000000000002"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "General event discussion for all participants", 0, new Guid("22222222-2222-2222-2222-222222222002"), null, "comments", true, true, null, null, "Event Chat", null },
                    { new Guid("aaaaaaaa-0001-0001-0001-000000000003"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "General event discussion for all participants", 0, new Guid("22222222-2222-2222-2222-222222222003"), null, "comments", true, true, null, null, "Event Chat", null },
                    { new Guid("aaaaaaaa-0001-0001-0001-000000000004"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "General event discussion for all participants", 0, new Guid("22222222-2222-2222-2222-222222222004"), null, "comments", true, true, null, null, "Event Chat", null },
                    { new Guid("aaaaaaaa-0001-0001-0001-000000000005"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "General event discussion for all participants", 0, new Guid("22222222-2222-2222-2222-222222222005"), null, "comments", true, true, null, null, "Event Chat", null },
                    { new Guid("aaaaaaaa-0002-0002-0002-000000000001"), 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "Important announcements from event leadership", 1, new Guid("00000000-0000-0000-0000-000000000001"), null, "bullhorn", true, false, null, null, "Announcements", null },
                    { new Guid("aaaaaaaa-0002-0002-0002-000000000002"), 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "Important announcements from event leadership", 1, new Guid("22222222-2222-2222-2222-222222222002"), null, "bullhorn", true, false, null, null, "Announcements", null },
                    { new Guid("aaaaaaaa-0002-0002-0002-000000000003"), 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "Important announcements from event leadership", 1, new Guid("22222222-2222-2222-2222-222222222003"), null, "bullhorn", true, false, null, null, "Announcements", null },
                    { new Guid("aaaaaaaa-0002-0002-0002-000000000004"), 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "Important announcements from event leadership", 1, new Guid("22222222-2222-2222-2222-222222222004"), null, "bullhorn", true, false, null, null, "Announcements", null },
                    { new Guid("aaaaaaaa-0002-0002-0002-000000000005"), 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system@cobra.mil", "Important announcements from event leadership", 1, new Guid("22222222-2222-2222-2222-222222222005"), null, "bullhorn", true, false, null, null, "Announcements", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0001-0001-0001-000000000001"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0001-0001-0001-000000000002"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0001-0001-0001-000000000003"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0001-0001-0001-000000000004"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0001-0001-0001-000000000005"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0002-0002-0002-000000000001"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0002-0002-0002-000000000002"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0002-0002-0002-000000000003"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0002-0002-0002-000000000004"));

            migrationBuilder.DeleteData(
                table: "ChatThreads",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0002-0002-0002-000000000005"));
        }
    }
}
