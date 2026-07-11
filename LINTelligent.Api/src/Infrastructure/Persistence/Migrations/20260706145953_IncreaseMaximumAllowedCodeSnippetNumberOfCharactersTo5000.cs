using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LINTelligent.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseMaximumAllowedCodeSnippetNumberOfCharactersTo5000 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CodeSnippet",
                table: "Reviews",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CodeSnippet",
                table: "Reviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);
        }
    }
}
