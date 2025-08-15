# Check and Start Services for Local Development
Write-Host "Checking Services for Local Development..." -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Check if PostgreSQL is running
Write-Host "Checking PostgreSQL..." -ForegroundColor Yellow
try {
    $pgProcess = Get-Process -Name "postgres" -ErrorAction SilentlyContinue
    if ($pgProcess) {
        Write-Host "✅ PostgreSQL is running (PID: $($pgProcess.Id))" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL is NOT running" -ForegroundColor Red
        Write-Host "   You need to start PostgreSQL service" -ForegroundColor Yellow
        Write-Host "   Options:" -ForegroundColor White
        Write-Host "   1. Start PostgreSQL service: Start-Service postgresql-x64-15" -ForegroundColor Gray
        Write-Host "   2. Or use Docker: docker-compose up postgres" -ForegroundColor Gray
        Write-Host "   3. Or install PostgreSQL locally" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Error checking PostgreSQL: $($_.Exception.Message)" -ForegroundColor Red
}

# Check if Redis is running
Write-Host ""
Write-Host "Checking Redis..." -ForegroundColor Yellow
try {
    $redisProcess = Get-Process -Name "redis-server" -ErrorAction SilentlyContinue
    if ($redisProcess) {
        Write-Host "✅ Redis is running (PID: $($redisProcess.Id))" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis is NOT running" -ForegroundColor Red
        Write-Host "   You need to start Redis service" -ForegroundColor Yellow
        Write-Host "   Options:" -ForegroundColor White
        Write-Host "   1. Use Docker: docker run -d -p 6379:6379 redis:alpine" -ForegroundColor Gray
        Write-Host "   2. Or install Redis locally" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Error checking Redis: $($_.Exception.Message)" -ForegroundColor Red
}

# Check if MongoDB is running
Write-Host ""
Write-Host "Checking MongoDB..." -ForegroundColor Yellow
try {
    $mongoProcess = Get-Process -Name "mongod" -ErrorAction SilentlyContinue
    if ($mongoProcess) {
        Write-Host "✅ MongoDB is running (PID: $($mongoProcess.Id))" -ForegroundColor Green
    } else {
        Write-Host "❌ MongoDB is NOT running" -ForegroundColor Red
        Write-Host "   You need to start MongoDB service" -ForegroundColor Yellow
        Write-Host "   Options:" -ForegroundColor White
        Write-Host "   1. Use Docker: docker run -d -p 27017:27017 mongo:latest" -ForegroundColor Gray
        Write-Host "   2. Or install MongoDB locally" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Error checking MongoDB: $($_.Exception.Message)" -ForegroundColor Red
}

# Check if RabbitMQ is running
Write-Host ""
Write-Host "Checking RabbitMQ..." -ForegroundColor Yellow
try {
    $rabbitProcess = Get-Process -Name "erl" -ErrorAction SilentlyContinue
    if ($rabbitProcess) {
        Write-Host "✅ RabbitMQ is running (PID: $($rabbitProcess.Id))" -ForegroundColor Green
    } else {
        Write-Host "❌ RabbitMQ is NOT running" -ForegroundColor Red
        Write-Host "   You need to start RabbitMQ service" -ForegroundColor Yellow
        Write-Host "   Options:" -ForegroundColor White
        Write-Host "   1. Use Docker: docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:management" -ForegroundColor Gray
        Write-Host "   2. Or install RabbitMQ locally" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Error checking RabbitMQ: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "=========" -ForegroundColor Cyan
Write-Host "If any services are missing, you have two options:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option 1: Use Docker (Recommended for development)" -ForegroundColor Green
Write-Host "  docker-compose up postgres redis mongodb rabbitmq" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 2: Install services locally" -ForegroundColor Green
Write-Host "  - PostgreSQL: https://www.postgresql.org/download/windows/" -ForegroundColor Gray
Write-Host "  - Redis: https://redis.io/download" -ForegroundColor Gray
Write-Host "  - MongoDB: https://www.mongodb.com/try/download/community" -ForegroundColor Gray
Write-Host "  - RabbitMQ: https://www.rabbitmq.com/download.html" -ForegroundColor Gray
