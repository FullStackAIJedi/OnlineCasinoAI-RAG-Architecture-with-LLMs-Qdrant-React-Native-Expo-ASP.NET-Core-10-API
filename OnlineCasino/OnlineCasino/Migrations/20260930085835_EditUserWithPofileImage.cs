using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCasino.Migrations
{
    /// <inheritdoc />
    public partial class EditUserWithPofileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PofileImage",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PofileImage",
                table: "Users");
        }
    }
}
