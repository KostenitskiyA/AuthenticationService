using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterGoogleUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "GoogleUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "GoogleId",
                table: "GoogleUsers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
