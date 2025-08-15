# Test Database Connection and Create Tables
Write-Host "Testing Database Connection..." -ForegroundColor Cyan

# Test if we can connect to PostgreSQL
try {
    # You'll need to install Npgsql or use a different method to test connection
    Write-Host "To test database connection, you need to:" -ForegroundColor Yellow
    Write-Host "1. Ensure PostgreSQL is running" -ForegroundColor White
    Write-Host "2. Check connection string in appsettings.Development.json" -ForegroundColor White
    Write-Host "3. Restart your API application" -ForegroundColor White
    
    Write-Host ""
    Write-Host "Current connection string from appsettings.Development.json:" -ForegroundColor Cyan
    $appSettings = Get-Content "src\Zentry.API\appsettings.Development.json" -Raw | ConvertFrom-Json
    Write-Host "PostgresConnection: $($appSettings.ConnectionStrings.PostgresConnection)" -ForegroundColor Gray
    
} catch {
    Write-Host "Error reading appsettings: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart your API application" -ForegroundColor White
Write-Host "2. Check API logs for database migration messages" -ForegroundColor White
Write-Host "3. Look for: 'FaceEmbeddings table created' or similar messages" -ForegroundColor White
Write-Host "4. Try calling the Face ID register API again" -ForegroundColor White

Write-Host ""
Write-Host "Expected behavior after restart:" -ForegroundColor Green
Write-Host "- FaceIdDbMigrationService will run on startup" -ForegroundColor Gray
Write-Host "- FaceEmbeddings table will be created" -ForegroundColor Gray
Write-Host "- FaceIdVerifyRequests table will be created" -ForegroundColor Gray
Write-Host "- API should work without 'relation does not exist' errors" -ForegroundColor Gray
