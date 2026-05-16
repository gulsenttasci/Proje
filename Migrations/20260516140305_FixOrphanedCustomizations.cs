using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanSpoon.Migrations
{
    /// <inheritdoc />
    public partial class FixOrphanedCustomizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomizationOption",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomizationGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomizationOption");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomizationGroup");
        }
    }
}
