using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMealAndDietaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DietaryRestrictions",
                table: "Rsvps");

            migrationBuilder.DropColumn(
                name: "MealPreference",
                table: "Rsvps");

            migrationBuilder.DropColumn(
                name: "PlusOneMealPreference",
                table: "Rsvps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DietaryRestrictions",
                table: "Rsvps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MealPreference",
                table: "Rsvps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlusOneMealPreference",
                table: "Rsvps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
