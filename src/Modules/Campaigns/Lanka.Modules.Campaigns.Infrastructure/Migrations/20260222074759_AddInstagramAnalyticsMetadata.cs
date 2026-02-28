using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddInstagramAnalyticsMetadata : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "instagram_metadata_audience_top_age_group",
            schema: "campaigns",
            table: "bloggers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "instagram_metadata_audience_top_country",
            schema: "campaigns",
            table: "bloggers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "instagram_metadata_audience_top_country_percentage",
            schema: "campaigns",
            table: "bloggers",
            type: "double precision",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "instagram_metadata_audience_top_gender",
            schema: "campaigns",
            table: "bloggers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "instagram_metadata_audience_top_gender_percentage",
            schema: "campaigns",
            table: "bloggers",
            type: "double precision",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "instagram_metadata_engagement_rate",
            schema: "campaigns",
            table: "bloggers",
            type: "double precision",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "instagram_metadata_audience_top_age_group",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "instagram_metadata_audience_top_country",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "instagram_metadata_audience_top_country_percentage",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "instagram_metadata_audience_top_gender",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "instagram_metadata_audience_top_gender_percentage",
            schema: "campaigns",
            table: "bloggers");

        migrationBuilder.DropColumn(
            name: "instagram_metadata_engagement_rate",
            schema: "campaigns",
            table: "bloggers");
    }
}
