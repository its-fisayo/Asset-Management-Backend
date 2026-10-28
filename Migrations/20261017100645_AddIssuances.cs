using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIssuances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IssuanceId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Issuances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issuances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issuances_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssuanceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuanceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuanceItems_Issuances_IssuanceId",
                        column: x => x.IssuanceId,
                        principalTable: "Issuances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IssuanceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_IssuanceId",
                table: "StockMovements",
                column: "IssuanceId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuanceItems_IssuanceId",
                table: "IssuanceItems",
                column: "IssuanceId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuanceItems_ProductId",
                table: "IssuanceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Issuances_IssuedByUserId",
                table: "Issuances",
                column: "IssuedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Issuances_IssuanceId",
                table: "StockMovements",
                column: "IssuanceId",
                principalTable: "Issuances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Issuances_IssuanceId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "IssuanceItems");

            migrationBuilder.DropTable(
                name: "Issuances");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_IssuanceId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "IssuanceId",
                table: "StockMovements");
        }
    }
}
