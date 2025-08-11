using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ConfigurationManagement.Migrations
{
    /// <inheritdoc />
    public partial class update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UserAttributes_AttributeDefinitions_AttributeId",
                table: "UserAttributes",
                column: "AttributeId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAttributes_AttributeDefinitions_AttributeId",
                table: "UserAttributes");
        }
    }
}
