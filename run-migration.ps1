# Script to run the new migration
Write-Host "Running FaceId migration to fix embedding field name..." -ForegroundColor Yellow

# Navigate to the FaceId project directory
Set-Location "src/Zentry.Modules/FaceId"

# Run the migration
try {
    # First, try to update the database with the new migration
    dotnet ef database update --context FaceIdDbContext
    Write-Host "Migration completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error running migration: $_" -ForegroundColor Red
    Write-Host "You may need to manually run the SQL commands in the migration file." -ForegroundColor Yellow
}

# Return to root directory
Set-Location "../../.."
