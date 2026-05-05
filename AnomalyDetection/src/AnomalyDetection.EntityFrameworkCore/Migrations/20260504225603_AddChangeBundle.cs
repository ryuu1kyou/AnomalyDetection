using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeBundle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppChangeBundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeatureId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DecisionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChangeType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ChangeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DocSyncStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DocVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_AppChangeBundles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppChangeBundleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeBundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChangeBundleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChangeBundleItems_AppChangeBundles_ChangeBundleId",
                        column: x => x.ChangeBundleId,
                        principalTable: "AppChangeBundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundleItems_ChangeBundleId",
                table: "AppChangeBundleItems",
                column: "ChangeBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundleItems_EntityId_EntityType",
                table: "AppChangeBundleItems",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundles_ChangeType",
                table: "AppChangeBundles",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundles_DecisionId",
                table: "AppChangeBundles",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundles_FeatureId",
                table: "AppChangeBundles",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChangeBundles_TenantId_FeatureId",
                table: "AppChangeBundles",
                columns: new[] { "TenantId", "FeatureId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppChangeBundleItems");

            migrationBuilder.DropTable(
                name: "AppChangeBundles");
        }
    }
}
