using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCasino.Migrations
{
    /// <inheritdoc />
    public partial class AddPayout4AndReelsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Payout4",
                table: "SlotSymbols",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Payout5",
                table: "SlotSymbols",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReelsCount",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payout4",
                table: "SlotSymbols");

            migrationBuilder.DropColumn(
                name: "Payout5",
                table: "SlotSymbols");

            migrationBuilder.DropColumn(
                name: "ReelsCount",
                table: "Games");
        }
    }
}
