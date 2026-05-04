using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignIntentToDetectionLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Assumptions",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommonalityStatus",
                table: "AppCanAnomalyDetectionLogics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Constraints",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionId",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignRationale",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocSyncStatus",
                table: "AppCanAnomalyDetectionLogics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DocVersion",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeatureId",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurposeShort",
                table: "AppCanAnomalyDetectionLogics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnknownResolutionDueDate",
                table: "AppCanAnomalyDetectionLogics",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_CommonalityStatus",
                table: "AppCanAnomalyDetectionLogics",
                column: "CommonalityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_DecisionId",
                table: "AppCanAnomalyDetectionLogics",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_DocSyncStatus",
                table: "AppCanAnomalyDetectionLogics",
                column: "DocSyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanAnomalyDetectionLogics_FeatureId",
                table: "AppCanAnomalyDetectionLogics",
                column: "FeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_CommonalityStatus",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_DecisionId",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_DocSyncStatus",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropIndex(
                name: "IX_AppCanAnomalyDetectionLogics_FeatureId",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "Assumptions",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "CommonalityStatus",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "Constraints",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "DecisionId",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "DesignRationale",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "DocSyncStatus",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "DocVersion",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "PurposeShort",
                table: "AppCanAnomalyDetectionLogics");

            migrationBuilder.DropColumn(
                name: "UnknownResolutionDueDate",
                table: "AppCanAnomalyDetectionLogics");
        }
    }
}
