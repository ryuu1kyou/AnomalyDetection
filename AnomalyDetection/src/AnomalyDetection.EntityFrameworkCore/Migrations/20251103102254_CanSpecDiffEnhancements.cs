using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnomalyDetection.Migrations
{
    /// <inheritdoc />
    public partial class CanSpecDiffEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangeCategory",
                table: "CanSpecDiff",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "CanSpecDiff",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpactedSubsystem",
                table: "CanSpecDiff",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "CanSpecDiff",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeCategory",
                table: "CanSpecDiff");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "CanSpecDiff");

            migrationBuilder.DropColumn(
                name: "ImpactedSubsystem",
                table: "CanSpecDiff");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "CanSpecDiff");
        }
    }
}
