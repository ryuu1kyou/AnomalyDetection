using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class SafetyTraceLinkPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppCanSignalMappings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "AppSafetyTraceLinkHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldLinkType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NewLinkType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSafetyTraceLinkHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSafetyTraceLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Relation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSafetyTraceLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinkHistories_ChangeTime",
                table: "AppSafetyTraceLinkHistories",
                column: "ChangeTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinkHistories_ChangeType",
                table: "AppSafetyTraceLinkHistories",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinkHistories_LinkId",
                table: "AppSafetyTraceLinkHistories",
                column: "LinkId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinks_CreationTime",
                table: "AppSafetyTraceLinks",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinks_LinkType",
                table: "AppSafetyTraceLinks",
                column: "LinkType");

            migrationBuilder.CreateIndex(
                name: "IX_AppSafetyTraceLinks_SourceRecordId_TargetRecordId",
                table: "AppSafetyTraceLinks",
                columns: new[] { "SourceRecordId", "TargetRecordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSafetyTraceLinkHistories");

            migrationBuilder.DropTable(
                name: "AppSafetyTraceLinks");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppCanSignalMappings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
