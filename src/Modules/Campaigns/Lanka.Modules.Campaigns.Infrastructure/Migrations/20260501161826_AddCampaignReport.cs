using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCampaignReport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "report_approach",
            schema: "campaigns",
            table: "campaigns",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "report_content_delivered",
            schema: "campaigns",
            table: "campaigns",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "report_notes",
            schema: "campaigns",
            table: "campaigns",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<string[]>(
            name: "report_post_permalinks",
            schema: "campaigns",
            table: "campaigns",
            type: "text[]",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "report_submitted_on_utc",
            schema: "campaigns",
            table: "campaigns",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "report_approach",
            schema: "campaigns",
            table: "campaigns");

        migrationBuilder.DropColumn(
            name: "report_content_delivered",
            schema: "campaigns",
            table: "campaigns");

        migrationBuilder.DropColumn(
            name: "report_notes",
            schema: "campaigns",
            table: "campaigns");

        migrationBuilder.DropColumn(
            name: "report_post_permalinks",
            schema: "campaigns",
            table: "campaigns");

        migrationBuilder.DropColumn(
            name: "report_submitted_on_utc",
            schema: "campaigns",
            table: "campaigns");
    }
}
