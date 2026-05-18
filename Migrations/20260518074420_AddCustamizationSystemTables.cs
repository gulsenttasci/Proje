using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanSpoon.Migrations
{
    /// <inheritdoc />
    public partial class AddCustamizationSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomizationGroup_MenuItems_MenuItemId",
                table: "CustomizationGroup");

            migrationBuilder.DropTable(
                name: "CustomizationOption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomizationGroup",
                table: "CustomizationGroup");

            migrationBuilder.RenameTable(
                name: "CustomizationGroup",
                newName: "CustomizationGroups");

            migrationBuilder.RenameIndex(
                name: "IX_CustomizationGroup_MenuItemId",
                table: "CustomizationGroups",
                newName: "IX_CustomizationGroups_MenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomizationGroups",
                table: "CustomizationGroups",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CustamizationOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomizationGroupId = table.Column<int>(type: "int", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustamizationOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustamizationOptions_CustomizationGroups_CustomizationGroupId",
                        column: x => x.CustomizationGroupId,
                        principalTable: "CustomizationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustamizationOptions_CustomizationGroupId",
                table: "CustamizationOptions",
                column: "CustomizationGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomizationGroups_MenuItems_MenuItemId",
                table: "CustomizationGroups",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomizationGroups_MenuItems_MenuItemId",
                table: "CustomizationGroups");

            migrationBuilder.DropTable(
                name: "CustamizationOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomizationGroups",
                table: "CustomizationGroups");

            migrationBuilder.RenameTable(
                name: "CustomizationGroups",
                newName: "CustomizationGroup");

            migrationBuilder.RenameIndex(
                name: "IX_CustomizationGroups_MenuItemId",
                table: "CustomizationGroup",
                newName: "IX_CustomizationGroup_MenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomizationGroup",
                table: "CustomizationGroup",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CustomizationOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomizationGroupId = table.Column<int>(type: "int", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomizationOption_CustomizationGroup_CustomizationGroupId",
                        column: x => x.CustomizationGroupId,
                        principalTable: "CustomizationGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationOption_CustomizationGroupId",
                table: "CustomizationOption",
                column: "CustomizationGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomizationGroup_MenuItems_MenuItemId",
                table: "CustomizationGroup",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
