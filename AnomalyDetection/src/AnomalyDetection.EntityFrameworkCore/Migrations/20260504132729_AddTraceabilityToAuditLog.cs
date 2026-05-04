using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceabilityToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChangeType",
                table: "AppAuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DecisionId",
                table: "AppAuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeatureId",
                table: "AppAuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_DecisionId",
                table: "AppAuditLogs",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAuditLogs_FeatureId",
                table: "AppAuditLogs",
                column: "FeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAuditLogs_DecisionId",
                table: "AppAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppAuditLogs_FeatureId",
                table: "AppAuditLogs");

            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "AppAuditLogs");

            migrationBuilder.DropColumn(
                name: "DecisionId",
                table: "AppAuditLogs");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "AppAuditLogs");
        }
    }
}
