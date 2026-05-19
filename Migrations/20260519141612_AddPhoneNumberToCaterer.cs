using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanSpoon.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToCaterer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Caterers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Caterers");
        }
    }
}
