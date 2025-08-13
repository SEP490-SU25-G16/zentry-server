using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Zentry.Modules.FaceId.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent creation to avoid failing when table already exists
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""FaceEmbeddings"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""Embedding"" vector(512) NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ""PK_FaceEmbeddings"" PRIMARY KEY (""Id"")
);");

            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_FaceEmbeddings_UserId"" ON ""FaceEmbeddings"" (""UserId"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FaceEmbeddings_UserId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""FaceEmbeddings"";");
        }
    }
}
