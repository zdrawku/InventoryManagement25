# Inventory Management 2025

A modern web API application for managing school equipment inventory built with ASP.NET Core 8.0.

## 📋 Overview

This application provides a comprehensive solution for tracking and managing educational equipment across different locations within a school environment. It offers a RESTful API for equipment management with features like condition tracking, status monitoring, and location-based organization.

## 🚀 Features

- **Equipment Management**: Full CRUD operations for equipment items
- **Status Tracking**: Monitor equipment availability (Available, Unavailable, Under Repair)
- **Condition Assessment**: Track physical condition (Excellent, Good, Fair, Damaged)
- **Location Management**: Organize equipment by room or location
- **Serial Number Tracking**: Maintain unique identifiers for each item
- **Photo Support**: Store equipment photo URLs for visual identification
- **REST API**: Clean RESTful endpoints for integration
- **Swagger Documentation**: Interactive API documentation
- **Database Migrations**: Entity Framework Core with SQLite

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQLite with Entity Framework Core 9.0
- **Documentation**: Swagger/OpenAPI
- **Language**: C# with nullable reference types
- **Architecture**: Controller-based Web API

## 📦 Equipment Properties

Each equipment item includes:

- **Basic Information**: Name, Type, Serial Number
- **Status Management**: 
  - Condition: Excellent, Good, Fair, Damaged
  - Status: Available, Unavailable, Under Repair
- **Location**: Room or area assignment
- **Photo URL**: Visual reference link

## 🔧 Setup & Installation

### Prerequisites

- .NET 8.0 SDK
- Git (optional)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/zdrawku/InventoryManagement25.git
   cd InventoryManagement25
   ```

2. **Navigate to the project directory**
   ```bash
   cd InventoryManagement2025
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Start the application**
   ```bash
   # For HTTPS (recommended)
   dotnet run --launch-profile https
   
   # For HTTP only
   dotnet run --launch-profile http
   ```

## 🌐 API Endpoints

### Equipment Controller (`/api/Equipment`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Equipment` | Get all equipment |
| GET | `/api/Equipment/{id}` | Get equipment by ID |
| POST | `/api/Equipment` | Create new equipment |
| PUT | `/api/Equipment/{id}` | Update equipment |
| DELETE | `/api/Equipment/{id}` | Delete equipment |

### API Documentation

Once running, access the interactive API documentation at:
- **HTTPS**: https://localhost:7122/swagger
- **HTTP**: http://localhost:5220/swagger

## 📊 Sample Data

The application includes seed data with sample equipment:

- Dell Laptop (Excellent condition, Available)
- Epson Projector (Good condition, Under Repair)
- HP Desktop (Fair condition, Available)
- Canon Camera (Damaged condition, Unavailable)

## 🗃️ Database

The application uses SQLite for local development with the following features:

- **Auto-migration**: Database is created and updated automatically on startup
- **Seed data**: Sample equipment is populated on first run
- **Connection string**: Configured in `appsettings.json`

## 🔨 Development

### Project Structure

```
InventoryManagement2025/
├── Controllers/          # API controllers
├── Models/              # Data models and entities
├── Data/                # Database context and initialization
├── Migrations/          # Entity Framework migrations
├── Properties/          # Launch settings
└── Program.cs           # Application entry point
```

### Key Files

- `Models/Equipment.cs` - Equipment entity and enums
- `Models/DBContext.cs` - Entity Framework context
- `Controllers/EquipmentController.cs` - Equipment API endpoints
- `Data/DbInit.cs` - Database seeding logic

## 🚀 Deployment

The application is configured for both development and production environments:

- **Development**: Uses SQLite with detailed logging
- **Production**: Ready for deployment with configurable connection strings

## 🤝 Contributing

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is available under the MIT License.

## 📞 Support

For questions or support, please open an issue in the GitHub repository.

---

**Built with ❤️ for educational institutions**