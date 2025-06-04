# ECommerce Database Tests

A comprehensive test suite for an eCommerce application using Entity Framework Core with PostgreSQL, built with .NET 9 and NUnit.

## Features

- **Entity Framework Core**: Database operations with PostgreSQL
- **Comprehensive Testing**: CRUD operations, validation, and error handling
- **Docker Support**: Containerized testing environment
- **Test Isolation**: Proper setup and teardown for reliable tests
- **Database Management**: Automated PostgreSQL setup with Docker

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local development)

## Quick Start

### Using Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd ecommerce-db-tests
   ```

2. **Run tests with Docker Compose**
   ```bash
   docker-compose up --build
   ```

3. **View test results**
   ```bash
   # Test results will be available in ./TestResults directory
   ls TestResults/
   ```

### Local Development

1. **Start PostgreSQL**
   ```bash
   docker-compose up postgres -d
   ```

2. **Run tests locally**
   ```bash
   dotnet test
   ```

3. **Optional: Access pgAdmin**
   ```bash
   docker-compose --profile dev up pgadmin
   # Access at http://localhost:8080
   # Email: admin@example.com, Password: admin
   ```

## Project Structure

```
├── Database/
│   ├── ECommerceContext.cs     # Entity Framework context
│   ├── UserService.cs          # User service with CRUD operations
│   └── Models/                 # Entity models
├── Tests/
│   └── Tests.cs               # NUnit test suite
├── Dockerfile                 # Docker configuration
├── docker-compose.yml        # Multi-container setup
└── README.md
```

## Test Coverage

### User Management Tests
- ✅ **Read Operations**: Get all users, get user by ID
- ✅ **Update Operations**: Email, profile information, role, status
- ✅ **Batch Operations**: Multiple user updates
- ✅ **Validation**: Email format, required fields
- ✅ **Error Handling**: Non-existent users, invalid data
- ✅ **Concurrency**: Concurrent update scenarios

### Database Operations
- ✅ **Connection Management**: Proper resource disposal
- ✅ **Transaction Handling**: Rollback on errors
- ✅ **Data Integrity**: Foreign key constraints

## Configuration

### Database Connection

The application uses the following connection string format:
```
Host=postgres;Port=5432;Database=ecommerce_db;Username=ecommerce_user;Password=ecommerce_password
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | See docker-compose.yml |
| `POSTGRES_DB` | Database name | `ecommerce_db` |
| `POSTGRES_USER` | Database user | `ecommerce_user` |
| `POSTGRES_PASSWORD` | Database password | `ecommerce_password` |

## Docker Commands

### Run Tests
```bash
# Run all tests
docker-compose up --build

# Run tests and keep containers running
docker-compose up --build -d

# View test logs
docker-compose logs tests

# Cleanup
docker-compose down -v
```

### Development Commands
```bash
# Start only database
docker-compose up postgres -d

# Start with pgAdmin
docker-compose --profile dev up -d

# Stop services
docker-compose down

# Reset database
docker-compose down -v && docker-compose up postgres -d
```

## Local Development Setup

1. **Install dependencies**
   ```bash
   dotnet restore
   ```

2. **Build project**
   ```bash
   dotnet build
   ```

3. **Run specific tests**
   ```bash
   # Run all tests
   dotnet test

   # Run specific test
   dotnet test --filter "Should_Update_User_Email_Successfully"

   # Run with detailed output
   dotnet test --verbosity detailed
   ```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Testing Guidelines

- Each test should be independent and not rely on other tests
- Use descriptive test names that clearly indicate what is being tested
- Follow the Arrange-Act-Assert pattern
- Clean up test data when necessary
- Use meaningful assertions with clear error messages

## Troubleshooting

### Common Issues

**Database Connection Issues**
```bash
# Check if PostgreSQL is running
docker-compose ps

# View database logs
docker-compose logs postgres

# Reset database
docker-compose down -v && docker-compose up postgres -d
```

**Test Failures**
```bash
# Run tests with detailed output
dotnet test --verbosity detailed

# Check test results
cat TestResults/TestResults.trx
```

**Port Conflicts**
```bash
# Change ports in docker-compose.yml if needed
# Default ports: 5432 (PostgreSQL), 8080 (pgAdmin)
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
