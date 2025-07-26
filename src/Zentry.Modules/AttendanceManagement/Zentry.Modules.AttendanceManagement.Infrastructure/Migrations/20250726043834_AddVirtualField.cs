using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Sessions_SessionId",
                table: "Rounds",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Sessions_SessionId",
                table: "Rounds");
        }
    }
}
