using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisJsonToPolicyRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiskAgeScore",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "RiskDestinationScore",
                table: "PolicyRequests");

            migrationBuilder.RenameColumn(
                name: "RiskTierScore",
                table: "PolicyRequests",
                newName: "ResubmissionCount");

            migrationBuilder.RenameColumn(
                name: "RiskDurationScore",
                table: "PolicyRequests",
                newName: "MaxResubmissions");

            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisJson",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DestinationRiskMultiplier",
                table: "PolicyRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RequestedDocTypes",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskReasoning",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "PolicyRequestDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysisJson",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "DestinationRiskMultiplier",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "RequestedDocTypes",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "RiskReasoning",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "PolicyRequestDocuments");

            migrationBuilder.RenameColumn(
                name: "ResubmissionCount",
                table: "PolicyRequests",
                newName: "RiskTierScore");

            migrationBuilder.RenameColumn(
                name: "MaxResubmissions",
                table: "PolicyRequests",
                newName: "RiskDurationScore");

            migrationBuilder.AddColumn<int>(
                name: "RiskAgeScore",
                table: "PolicyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskDestinationScore",
                table: "PolicyRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
