using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanSpoon.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToCaterer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Caterers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Caterers",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Caterers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Caterers");
        }
    }
}
