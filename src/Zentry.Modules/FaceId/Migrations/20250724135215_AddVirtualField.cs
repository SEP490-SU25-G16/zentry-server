using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zentry.Modules.UserManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add FK only if it doesn't already exist (PostgreSQL doesn't support IF NOT EXISTS for constraints)
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint c
        JOIN pg_class t ON t.oid = c.conrelid
        WHERE c.conname = 'FK_Users_Accounts_AccountId'
          AND t.relname = 'Users'
    ) THEN
        ALTER TABLE ""Users""
        ADD CONSTRAINT ""FK_Users_Accounts_AccountId""
        FOREIGN KEY (""AccountId"") REFERENCES ""Accounts""(""Id"") ON DELETE CASCADE;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP CONSTRAINT IF EXISTS \"FK_Users_Accounts_AccountId\";");
        }
    }
}