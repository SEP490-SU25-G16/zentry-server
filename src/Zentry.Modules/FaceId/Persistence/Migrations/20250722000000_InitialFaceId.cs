using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Zentry.Modules.FaceId.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFaceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pgvector extension
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.CreateTable(
                name: "FaceEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(512)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceEmbeddings_UserId",
                table: "FaceEmbeddings",
                column: "UserId",
                unique: true);

            // Create vector index using raw SQL since EF Core migrations don't support vector index creation directly
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_FaceEmbeddings_Embedding"" ON ""FaceEmbeddings"" USING ivfflat (""Embedding"" vector_cosine_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceEmbeddings");
        }
    }
} 