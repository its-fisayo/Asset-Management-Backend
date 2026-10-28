
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagementBackend.Migrations
{
    public partial class AddCategoriesAndBrands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the Brands table.
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            // 2. Create the Categories table.
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            // 3. Add nullable foreign-key columns first.
            // Existing products can remain in the table while we migrate them.
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            // 4. Copy existing brand names into the Brands table.
            // Blank names are assigned a sensible fallback.
            migrationBuilder.Sql(
                """
                INSERT INTO [Brands] ([Id], [Name])
                SELECT NEWID(), normalized.[Name]
                FROM
                (
                    SELECT
                        CASE
                            WHEN NULLIF(
                                LTRIM(RTRIM([Brand])), ''
                            ) IS NULL
                            THEN N'Unknown'
                            ELSE LTRIM(RTRIM([Brand]))
                        END AS [Name]
                    FROM [Products]
                ) AS normalized
                GROUP BY normalized.[Name];
                """);

            // 5. Copy existing category names into Categories.
            migrationBuilder.Sql(
                """
                INSERT INTO [Categories] ([Id], [Name])
                SELECT NEWID(), normalized.[Name]
                FROM
                (
                    SELECT
                        CASE
                            WHEN NULLIF(
                                LTRIM(RTRIM([Category])), ''
                            ) IS NULL
                            THEN N'Uncategorized'
                            ELSE LTRIM(RTRIM([Category]))
                        END AS [Name]
                    FROM [Products]
                ) AS normalized
                GROUP BY normalized.[Name];
                """);

            // 6. Assign each existing product its matching BrandId.
            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.[BrandId] = b.[Id]
                FROM [Products] AS p
                INNER JOIN [Brands] AS b
                    ON b.[Name] =
                        CASE
                            WHEN NULLIF(
                                LTRIM(RTRIM(p.[Brand])), ''
                            ) IS NULL
                            THEN N'Unknown'
                            ELSE LTRIM(RTRIM(p.[Brand]))
                        END;
                """);

            // 7. Assign each existing product its matching CategoryId.
            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.[CategoryId] = c.[Id]
                FROM [Products] AS p
                INNER JOIN [Categories] AS c
                    ON c.[Name] =
                        CASE
                            WHEN NULLIF(
                                LTRIM(RTRIM(p.[Category])), ''
                            ) IS NULL
                            THEN N'Uncategorized'
                            ELSE LTRIM(RTRIM(p.[Category]))
                        END;
                """);

            // 8. Make the foreign-key columns required.
            migrationBuilder.AlterColumn<Guid>(
                name: "BrandId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // 9. Create indexes.
            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            // 10. Add foreign-key constraints.
            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 11. Remove the old string columns only after migration.
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the old string columns as nullable temporarily.
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            // Restore brand names from the related Brand records.
            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.[Brand] = b.[Name]
                FROM [Products] AS p
                INNER JOIN [Brands] AS b
                    ON p.[BrandId] = b.[Id];
                """);

            // Restore category names from the related Category records.
            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.[Category] = c.[Name]
                FROM [Products] AS p
                INNER JOIN [Categories] AS c
                    ON p.[CategoryId] = c.[Id];
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Remove the new relationships and indexes.
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Name",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
