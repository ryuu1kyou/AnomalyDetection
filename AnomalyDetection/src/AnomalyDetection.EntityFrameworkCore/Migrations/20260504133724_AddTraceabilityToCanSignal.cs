using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceabilityToCanSignal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommonalityStatus",
                table: "AppCanSignals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DecisionId",
                table: "AppCanSignals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeatureId",
                table: "AppCanSignals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnknownResolutionDueDate",
                table: "AppCanSignals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_CommonalityStatus",
                table: "AppCanSignals",
                column: "CommonalityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppCanSignals_FeatureId",
                table: "AppCanSignals",
                column: "FeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_CommonalityStatus",
                table: "AppCanSignals");

            migrationBuilder.DropIndex(
                name: "IX_AppCanSignals_FeatureId",
                table: "AppCanSignals");

            migrationBuilder.DropColumn(
                name: "CommonalityStatus",
                table: "AppCanSignals");

            migrationBuilder.DropColumn(
                name: "DecisionId",
                table: "AppCanSignals");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "AppCanSignals");

            migrationBuilder.DropColumn(
                name: "UnknownResolutionDueDate",
                table: "AppCanSignals");
        }
    }
}
