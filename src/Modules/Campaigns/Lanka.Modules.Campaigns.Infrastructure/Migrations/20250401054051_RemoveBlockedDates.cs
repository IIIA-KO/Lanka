﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lanka.Modules.Campaigns.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlockedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocked_dates",
                schema: "campaigns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blocked_dates",
                schema: "campaigns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blogger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blocked_dates", x => x.id);
                    table.ForeignKey(
                        name: "fk_blocked_dates_bloggers_blogger_id",
                        column: x => x.blogger_id,
                        principalSchema: "campaigns",
                        principalTable: "bloggers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blocked_dates_blogger_id_date",
                schema: "campaigns",
                table: "blocked_dates",
                columns: new[] { "blogger_id", "date" },
                unique: true);
        }
    }
}
