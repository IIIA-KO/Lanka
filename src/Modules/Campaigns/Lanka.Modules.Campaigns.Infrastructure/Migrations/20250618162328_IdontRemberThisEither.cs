using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IdontRemberThisEither : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_photo_id",
                schema: "campaigns",
                table: "bloggers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_photo_uri",
                schema: "campaigns",
                table: "bloggers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_photo_id",
                schema: "campaigns",
                table: "bloggers");

            migrationBuilder.DropColumn(
                name: "profile_photo_uri",
                schema: "campaigns",
                table: "bloggers");
        }
    }
}
