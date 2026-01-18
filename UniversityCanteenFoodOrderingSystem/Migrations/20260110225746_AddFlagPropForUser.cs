using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityCanteenFoodOrderingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFlagPropForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByUser",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletedByUser",
                table: "Orders");
        }
    }
}
