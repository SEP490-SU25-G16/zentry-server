using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.ConfigurationManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatevirtualattribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Configurations_AttributeDefinitions_AttributeId",
                table: "Configurations",
                column: "AttributeId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Configurations_AttributeDefinitions_AttributeId",
                table: "Configurations");
        }
    }
}
