using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessoriesStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingAddressSnapshotToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingAdditionalDetails",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingApartmentNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingArea",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingBuildingNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingFullName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhoneNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAdditionalDetails",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingApartmentNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingArea",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingBuildingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFullName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPhoneNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingStreet",
                table: "Orders");
        }
    }
}
