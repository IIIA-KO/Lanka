using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPayoutAccount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "payout_currency",
            schema: "campaigns",
            table: "bloggers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "payout_iban",
            schema: "campaigns",
            table: "bloggers",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "payout_currency",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "payout_iban",
            schema: "campaigns",
            table: "bloggers");
    }
}
