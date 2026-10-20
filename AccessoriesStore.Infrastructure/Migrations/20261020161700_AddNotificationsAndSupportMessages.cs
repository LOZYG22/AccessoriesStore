using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessoriesStore.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261020161700_AddNotificationsAndSupportMessages")]
public partial class AddNotificationsAndSupportMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PaymentAttempts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                OrderId = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                PaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                PaymobOrderId = table.Column<long>(type: "bigint", nullable: true),
                PaymobTransactionId = table.Column<long>(type: "bigint", nullable: true),
                AmountCents = table.Column<long>(type: "bigint", nullable: false),
                RefundedAmountCents = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PaymentAttempts", x => x.Id);
                table.ForeignKey("FK_PaymentAttempts_Orders_OrderId", x => x.OrderId, "Orders", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table => table.PrimaryKey("PK_Notifications", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SupportConversations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Subject = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_SupportConversations", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SupportMessages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ConversationId = table.Column<int>(type: "int", nullable: false),
                SenderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                SenderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsReadByCustomer = table.Column<bool>(type: "bit", nullable: false),
                IsReadByAdmin = table.Column<bool>(type: "bit", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SupportMessages", x => x.Id);
                table.ForeignKey("FK_SupportMessages_SupportConversations_ConversationId", x => x.ConversationId, "SupportConversations", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Notifications_UserId_ReadAt_CreatedAt", "Notifications", new[] { "UserId", "ReadAt", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_PaymentAttempts_OrderId_CreatedAt", "PaymentAttempts", new[] { "OrderId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_PaymentAttempts_PaymobOrderId", "PaymentAttempts", "PaymobOrderId");
        migrationBuilder.CreateIndex("IX_PaymentAttempts_PaymentIntentId", "PaymentAttempts", "PaymentIntentId", unique: true, filter: "[PaymentIntentId] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_SupportConversations_CustomerId_UpdatedAt", "SupportConversations", new[] { "CustomerId", "UpdatedAt" });
        migrationBuilder.CreateIndex("IX_SupportConversations_Status_UpdatedAt", "SupportConversations", new[] { "Status", "UpdatedAt" });
        migrationBuilder.CreateIndex("IX_SupportMessages_ConversationId_CreatedAt", "SupportMessages", new[] { "ConversationId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_SupportMessages_ConversationId_IsReadByAdmin_IsAdmin", "SupportMessages", new[] { "ConversationId", "IsReadByAdmin", "IsAdmin" });
        migrationBuilder.CreateIndex("IX_SupportMessages_ConversationId_IsReadByCustomer_IsAdmin", "SupportMessages", new[] { "ConversationId", "IsReadByCustomer", "IsAdmin" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SupportMessages");
        migrationBuilder.DropTable("Notifications");
        migrationBuilder.DropTable("SupportConversations");
        migrationBuilder.DropTable("PaymentAttempts");
    }
}
