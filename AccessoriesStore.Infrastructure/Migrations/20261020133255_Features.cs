using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessoriesStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = 'IX_Orders_PaymentIntentId'
              AND object_id = OBJECT_ID('Orders')
        )
        DROP INDEX [IX_Orders_PaymentIntentId] ON [Orders];
    ");

            migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = 'IX_Orders_PaymobOrderId'
              AND object_id = OBJECT_ID('Orders')
        )
        DROP INDEX [IX_Orders_PaymobOrderId] ON [Orders];
    ");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentIntentId",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentIntentId",
                table: "Orders",
                column: "PaymentIntentId",
                unique: true,
                filter: "[PaymentIntentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymobOrderId",
                table: "Orders",
                column: "PaymobOrderId",
                unique: true,
                filter: "[PaymobOrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentIntentId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymobOrderId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentIntentId",
                table: "Orders",
                column: "PaymentIntentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymobOrderId",
                table: "Orders",
                column: "PaymobOrderId",
                unique: true);
        }
    }
}
