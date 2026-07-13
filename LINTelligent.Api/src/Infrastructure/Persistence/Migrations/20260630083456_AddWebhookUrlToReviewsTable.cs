using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LINTelligent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookUrlToReviewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Reviews",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "Reviews");
        }
    }
}
