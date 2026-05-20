using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingCampaignExpiryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_campaigns_pending_scheduled_on_utc",
                schema: "campaigns",
                table: "campaigns",
                column: "scheduled_on_utc",
                filter: "status = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_campaigns_pending_scheduled_on_utc",
                schema: "campaigns",
                table: "campaigns");
        }
    }
}
