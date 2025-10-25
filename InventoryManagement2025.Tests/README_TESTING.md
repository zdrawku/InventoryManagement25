# Test Execution Guide for Inventory Management 2025

## Overview
This document provides guidance on running the comprehensive test suite for the Inventory Management 2025 API.

## Test Categories

### 1. Unit Tests
- **AuthControllerTests**: Authentication, registration, role management
- **EquipmentControllerTests**: Equipment CRUD operations, search, export
- **EquipmentRequestsControllerTests**: Request lifecycle, approval workflow
- **DocumentsControllerTests**: Document management, permissions
- **ReportsControllerTests**: Usage statistics, history, exports

### 2. Integration Tests
- **IntegrationTests**: End-to-end API workflows, authentication flows

## Running Tests

### Prerequisites
```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build
```

### Run All Tests
```bash
# From the solution root
dotnet test

# From the test project
cd InventoryManagement2025.Tests
dotnet test
```

### Run Specific Test Categories
```bash
# Run only unit tests (exclude integration tests)
dotnet test --filter "FullyQualifiedName!~IntegrationTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run specific test class
dotnet test --filter "ClassName=AuthControllerTests"
```

### Run Tests with Coverage
```bash
# Install coverage tool if not already installed
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

# Open coverage report
# Navigate to TestResults/CoverageReport/index.html
```

### Verbose Output
```bash
# Run with detailed output
dotnet test --verbosity detailed

# Run with logger
dotnet test --logger "console;verbosity=detailed"
```

## Test Coverage Areas

### Core Functionalities Covered

#### 1. Authentication & Authorization ✅
- User registration and login
- JWT token generation and validation
- Role-based access control (Admin/User)
- User management (list users, update roles)
- Logout functionality

#### 2. Equipment Management ✅
- CRUD operations (Create, Read, Update, Delete)
- Equipment search with multiple filters
- Status updates (Available, CheckedOut, UnderRepair)
- CSV export functionality
- Input validation and error handling

#### 3. Equipment Request Workflow ✅
- Request creation with time validation
- Automatic approval for non-sensitive equipment
- Manual approval workflow for sensitive equipment
- Request approval and rejection
- Equipment return processing with condition tracking
- Overlap prevention for equipment requests

#### 4. Document Management ✅
- Document upload and storage
- Role-based document visibility
- Document CRUD operations
- Permission-based access control
- Owner-based modification rights

#### 5. Reporting System ✅
- Usage statistics generation
- Activity history tracking
- Data export capabilities
- Admin-only access control

#### 6. Security & Permissions ✅
- Authentication middleware testing
- Authorization attribute validation
- Role-based endpoint protection
- Input sanitization and validation

### Integration Testing ✅
- Complete API workflow testing
- Multi-step business processes
- Cross-controller functionality
- Database state verification
- Error handling and edge cases

## Test Statistics

### Test Count by Category
- Authentication Tests: 11 tests
- Equipment Tests: 16 tests
- Equipment Requests Tests: 12 tests
- Documents Tests: 12 tests
- Reports Tests: 7 tests
- Integration Tests: 8 tests
- **Total: ~66 comprehensive tests**

### Coverage Areas
- Controllers: 100% method coverage
- Business logic: Core workflows covered
- Error handling: Exception scenarios tested
- Security: Authentication/authorization tested
- Data validation: Input validation covered

## Expected Test Results

All tests should pass when run against a clean database. The tests use in-memory databases to ensure isolation and fast execution.

### Common Issues and Solutions

1. **Test Database Conflicts**
   - Each test uses a unique in-memory database
   - Tests are isolated and should not affect each other

2. **Authentication Issues**
   - Tests automatically create required users and roles
   - JWT tokens are generated for testing purposes

3. **Time-Sensitive Tests**
   - Equipment requests use relative dates
   - Tests account for timing variations

4. **Missing Dependencies**
   - Ensure all NuGet packages are restored
   - Check that test project references main project

## Continuous Integration

For CI/CD pipelines, use:
```bash
# Run tests with JUnit output for CI systems
dotnet test --logger "junit;LogFilePath=test-results.xml"

# Run tests with coverage for CI
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Test Maintenance

### Adding New Tests
1. Inherit from `TestBase` for unit tests
2. Use `WebApplicationFactory<Program>` for integration tests
3. Follow AAA pattern (Arrange, Act, Assert)
4. Include both positive and negative test cases

### Best Practices
- Test one thing at a time
- Use descriptive test names
- Include edge cases and error scenarios
- Verify both success and failure paths
- Test security boundaries

---

**Note**: This test suite provides comprehensive coverage of the core API functionalities. Run these tests regularly during development to ensure code quality and catch regressions early.