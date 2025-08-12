using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogapp.Migrations
{
    /// <inheritdoc />
    public partial class FixedBookmarkCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_BlogPosts_BlogPostId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_BlogPostId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "BlogPostId",
                table: "Bookmarks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogPostId",
                table: "Bookmarks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_BlogPostId",
                table: "Bookmarks",
                column: "BlogPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_BlogPosts_BlogPostId",
                table: "Bookmarks",
                column: "BlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
