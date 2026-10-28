using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIssuanceIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "IssuanceItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "IssuanceItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_IssuanceItems_Barcode",
                table: "IssuanceItems",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssuanceItems_SerialNumber",
                table: "IssuanceItems",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IssuanceItems_Barcode",
                table: "IssuanceItems");

            migrationBuilder.DropIndex(
                name: "IX_IssuanceItems_SerialNumber",
                table: "IssuanceItems");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "IssuanceItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "IssuanceItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
