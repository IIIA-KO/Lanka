using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddNotifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            schema: "campaigns",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                recipient_blogger_id = table.Column<Guid>(type: "uuid", nullable: false),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                is_read = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_notifications", x => x.id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "notifications",
            schema: "campaigns");
    }
}
