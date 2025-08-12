using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogapp.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogAccessStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogAccessStatus",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlogAccessStatus",
                table: "Users");
        }
    }
}
