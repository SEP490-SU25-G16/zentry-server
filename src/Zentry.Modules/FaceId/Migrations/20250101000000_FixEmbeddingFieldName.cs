using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.FaceId.Migrations
{
    /// <inheritdoc />
    public partial class FixEmbeddingFieldName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing vector column and create the new encrypted embedding column
            migrationBuilder.Sql(@"
                -- Drop the existing vector column
                ALTER TABLE ""FaceEmbeddings"" DROP COLUMN IF EXISTS ""Embedding"";
                
                -- Add the new encrypted embedding column
                ALTER TABLE ""FaceEmbeddings"" ADD COLUMN ""EncryptedEmbedding"" bytea;
                
                -- Update the column to be nullable initially (can be made required later if needed)
                ALTER TABLE ""FaceEmbeddings"" ALTER COLUMN ""EncryptedEmbedding"" SET NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to the original vector column
            migrationBuilder.Sql(@"
                -- Drop the encrypted embedding column
                ALTER TABLE ""FaceEmbeddings"" DROP COLUMN IF EXISTS ""EncryptedEmbedding"";
                
                -- Recreate the original vector column
                ALTER TABLE ""FaceEmbeddings"" ADD COLUMN ""Embedding"" vector(512) NOT NULL;
            ");
        }
    }
}
