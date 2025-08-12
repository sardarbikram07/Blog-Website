using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogapp.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoToBlogPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoPath",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoPath",
                table: "BlogPosts");
        }
    }
}
