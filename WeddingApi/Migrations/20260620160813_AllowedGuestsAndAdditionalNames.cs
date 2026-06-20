using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingApi.Migrations
{
    /// <inheritdoc />
    public partial class AllowedGuestsAndAdditionalNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlusOneAttending",
                table: "Rsvps");

            migrationBuilder.DropColumn(
                name: "PlusOneName",
                table: "Rsvps");

            migrationBuilder.DropColumn(
                name: "AllowedPlusOne",
                table: "Guests");

            migrationBuilder.AddColumn<List<string>>(
                name: "AdditionalGuests",
                table: "Rsvps",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<int>(
                name: "AllowedGuests",
                table: "Guests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalGuests",
                table: "Rsvps");

            migrationBuilder.DropColumn(
                name: "AllowedGuests",
                table: "Guests");

            migrationBuilder.AddColumn<bool>(
                name: "PlusOneAttending",
                table: "Rsvps",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlusOneName",
                table: "Rsvps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AllowedPlusOne",
                table: "Guests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
