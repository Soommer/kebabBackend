using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kebabBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsProcessedToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "carts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "carts");
        }
    }
}
