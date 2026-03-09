using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyRequestEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyRequests",
                columns: table => new
                {
                    PolicyRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TravellerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TravellerAge = table.Column<int>(type: "int", nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KycType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KycNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskAgeScore = table.Column<int>(type: "int", nullable: false),
                    RiskDestinationScore = table.Column<int>(type: "int", nullable: false),
                    RiskDurationScore = table.Column<int>(type: "int", nullable: false),
                    RiskTierScore = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRequests", x => x.PolicyRequestId);
                    table.ForeignKey(
                        name: "FK_PolicyRequests_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyRequests_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyRequests_PolicyProducts_PolicyProductId",
                        column: x => x.PolicyProductId,
                        principalTable: "PolicyProducts",
                        principalColumn: "PolicyProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyRequestDocuments",
                columns: table => new
                {
                    PolicyRequestDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyRequestId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRequestDocuments", x => x.PolicyRequestDocumentId);
                    table.ForeignKey(
                        name: "FK_PolicyRequestDocuments_PolicyRequests_PolicyRequestId",
                        column: x => x.PolicyRequestId,
                        principalTable: "PolicyRequests",
                        principalColumn: "PolicyRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRequestDocuments_PolicyRequestId",
                table: "PolicyRequestDocuments",
                column: "PolicyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRequests_AgentId",
                table: "PolicyRequests",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRequests_CustomerId",
                table: "PolicyRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRequests_PolicyProductId",
                table: "PolicyRequests",
                column: "PolicyProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyRequestDocuments");

            migrationBuilder.DropTable(
                name: "PolicyRequests");
        }
    }
}
