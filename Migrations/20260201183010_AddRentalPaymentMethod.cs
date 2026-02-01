using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDealership.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayMethod",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayMethod",
                table: "Rentals");
        }
    }
}
