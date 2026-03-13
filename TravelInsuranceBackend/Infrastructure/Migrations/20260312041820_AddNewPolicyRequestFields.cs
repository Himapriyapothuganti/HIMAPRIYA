using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPolicyRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dependents",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TripFrequency",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniversityName",
                table: "PolicyRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dependents",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "TripFrequency",
                table: "PolicyRequests");

            migrationBuilder.DropColumn(
                name: "UniversityName",
                table: "PolicyRequests");
        }
    }
}
