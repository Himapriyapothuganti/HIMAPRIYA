using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class claimsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TravellerAge",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TravellerName",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyProductId",
                table: "Policies",
                column: "PolicyProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_PolicyProducts_PolicyProductId",
                table: "Policies",
                column: "PolicyProductId",
                principalTable: "PolicyProducts",
                principalColumn: "PolicyProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_PolicyProducts_PolicyProductId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PolicyProductId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "TravellerAge",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "TravellerName",
                table: "Policies");
        }
    }
}
