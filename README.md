# Zentry - Environment Variables

Tạo file `.env` ở thư mục gốc của project (cùng cấp với `docker-compose.yml`) và thêm nội dung sau:

```env
# =========================
# ASP.NET Core
# =========================
ASPNETCORE_ENVIRONMENT=Development

# =========================
# PostgreSQL
# =========================
POSTGRES_DB=zentry
POSTGRES_USER=admin
POSTGRES_PASSWORD=pass

# =========================
# ConnectionStrings
# =========================
FACEID_CONNECTION=Host=postgres;Port=5432;Database=zentry;Username=admin;Password=pass;
# Only for dev
# DEFAULT_CONNECTION=Host=postgres;Port=5432;Database=zentry;Username=admin;Password=pass;

DEFAULT_CONNECTION=Host=zentry.c2pgywcgsk5m.us-east-1.rds.amazonaws.com;Port=5432;Database=zentry;Username=postgres;Password=zentry_owner;
# =========================
# Redis
# =========================
REDIS_CONNECTION=redis:6379

# =========================
# MongoDB
# =========================
MONGO_CONNECTION=

# =========================
# RabbitMQ
# =========================
RABBITMQ_CONNECTION=amqp://admin:pass@rabbitmq:5672/

# =========================
# JWT
# =========================
JWT_SECRET=a-super-secret-key-that-is-long-enough-for-hs256-algorithm
JWT_ISSUER=https://zentry.local
JWT_AUDIENCE=https://zentry.local
JWT_EXPIRATION_MINUTES=60
