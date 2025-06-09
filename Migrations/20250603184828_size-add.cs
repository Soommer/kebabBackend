using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kebabBackend.Migrations
{
    /// <inheritdoc />
    public partial class sizeadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "cartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "cartItems");
        }
    }
}
