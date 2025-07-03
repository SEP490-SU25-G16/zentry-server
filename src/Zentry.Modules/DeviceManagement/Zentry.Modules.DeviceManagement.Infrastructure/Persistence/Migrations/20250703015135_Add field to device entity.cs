using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.DeviceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Addfieldtodeviceentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PushNotificationToken",
                table: "Devices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "PushNotificationToken",
                table: "Devices");
        }
    }
}
