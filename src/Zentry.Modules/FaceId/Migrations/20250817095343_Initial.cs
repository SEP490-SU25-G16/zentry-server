using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.FaceId.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaceEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedEmbedding = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaceIdVerifyRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Threshold = table.Column<float>(type: "real", nullable: false, defaultValue: 0.7f),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Matched = table.Column<bool>(type: "boolean", nullable: true),
                    Similarity = table.Column<float>(type: "real", nullable: true),
                    NotificationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceIdVerifyRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceEmbeddings_UserId",
                table: "FaceEmbeddings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceIdReq_ExpiresAt",
                table: "FaceIdVerifyRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_FaceIdReq_Group_Target_Status_Exp",
                table: "FaceIdVerifyRequests",
                columns: new[] { "RequestGroupId", "TargetUserId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FaceIdReq_Session_Status",
                table: "FaceIdVerifyRequests",
                columns: new[] { "SessionId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceEmbeddings");

            migrationBuilder.DropTable(
                name: "FaceIdVerifyRequests");
        }
    }
}
