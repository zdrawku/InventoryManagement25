# Inventory Management 2025

A full-stack web application for managing school equipment inventory built with ASP.NET Core 8.0 (Backend) and Angular 20 (Frontend).

## 📋 Overview

This application provides a comprehensive solution for tracking and managing educational equipment across different locations within a school environment. It features role-based access control, equipment request workflows, and detailed reporting capabilities.

## 🏗️ Architecture

- **Backend**: ASP.NET Core 8.0 Web API with SQLite database
- **Frontend**: Angular 20 standalone components with TypeScript
- **Communication**: RESTful API

## 🚀 Features

### Backend API
- **Equipment Management**: Full CRUD operations for equipment items
- **Status Tracking**: Monitor equipment availability (Available, Unavailable, Under Repair)
- **Condition Assessment**: Track physical condition (Excellent, Good, Fair, Damaged)
- **Location Management**: Organize equipment by room or location
- **Serial Number Tracking**: Maintain unique identifiers for each item
- **Photo Support**: Store equipment photo URLs for visual identification
- **REST API**: Clean RESTful endpoints for integration
- **Swagger Documentation**: Interactive API documentation
- **Database Migrations**: Entity Framework Core with SQLite

### Frontend Application
- **User Authentication**: Register, login, and role-based access control
- **Equipment Catalog**: Browse, search, and filter equipment
- **Request System**: Submit and track equipment requests
- **Admin Dashboard**: Manage equipment, approve requests, view reports
- **User Dashboard**: View available equipment and personal request history
- **Reports & Analytics**: Usage reports and history logs with export functionality
- **Responsive Design**: Modern UI with CSS styling

## 👥 User Roles

### Users
- View available equipment
- Request permitted items
- View personal borrowing history
- Cannot add/edit/delete equipment
- Cannot approve/reject requests
- Cannot access system settings or reports

### Administrators
- All User permissions plus:
- Manage users and permissions
- Add/update/delete equipment
- Approve/reject equipment requests
- Log returns and update item condition
- Export reports (CSV/PDF)
- Access system analytics and reporting

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: SQLite with Entity Framework Core 9.0
- **Documentation**: Swagger/OpenAPI
- **Language**: C# with nullable reference types
- **Architecture**: Controller-based Web API

### Frontend
- **Framework**: Angular 20
- **Language**: TypeScript 5.9
- **Build Tool**: Angular CLI
- **State Management**: Angular Signals
- **Styling**: CSS
- **HTTP Client**: Angular HttpClient

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
- Node.js 20.x or higher
- npm 10.x or higher
- Git (optional)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/zdrawku/InventoryManagement25.git
   cd InventoryManagement25
   ```

2. **Navigate to the backend project directory**
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

5. **Start the backend API**
   ```bash
   # For HTTPS (recommended)
   dotnet run --launch-profile https
   
   # For HTTP only
   dotnet run --launch-profile http
   ```

   The API will be available at:
   - HTTPS: https://localhost:7122
   - HTTP: http://localhost:5220
   - Swagger: https://localhost:7122/swagger

### Frontend Setup

1. **Navigate to the frontend directory**
   ```bash
   cd ../inventory-frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Update API URL** (if needed)
   
   Edit `src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5220/api'
   };
   ```

4. **Start the development server**
   ```bash
   npm start
   ```

   The frontend will be available at: http://localhost:4200

### Running Both Backend and Frontend

For full application functionality, run both servers:

1. Terminal 1 - Backend:
   ```bash
   cd InventoryManagement2025
   dotnet run --launch-profile https
   ```

2. Terminal 2 - Frontend:
   ```bash
   cd inventory-frontend
   npm start
   ```

Access the application at http://localhost:4200

## 🌐 API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | User registration |
| POST | `/auth/login` | User login |
| GET | `/auth/logout` | User logout |

### Equipment Controller (`/api/Equipment`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Equipment` | Get all equipment |
| GET | `/api/Equipment/{id}` | Get equipment by ID |
| POST | `/api/Equipment` | Create new equipment (Admin only) |
| PUT | `/api/Equipment/{id}` | Update equipment (Admin only) |
| PUT | `/api/Equipment/{id}/status` | Update equipment status (Admin only) |
| DELETE | `/api/Equipment/{id}` | Delete equipment (Admin only) |

### Requests (`/api/request`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/request` | Create equipment request |
| GET | `/requests` | Get user's requests |
| GET | `/manager/requests` | Get all requests (Admin only) |
| PUT | `/request/{id}/approve` | Approve request (Admin only) |
| PUT | `/request/{id}/reject` | Reject request (Admin only) |
| PUT | `/request/{id}/return` | Return equipment (Admin only) |

### Reports (`/api/reports`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/reports/usage` | Get usage report (Admin only) |
| GET | `/reports/history` | Get history logs (Admin only) |
| GET | `/reports/export` | Export reports (Admin only) |

### API Documentation

Once the backend is running, access the interactive API documentation at:
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

### Backend Project Structure

```
InventoryManagement2025/
├── Controllers/          # API controllers
├── Models/              # Data models and entities
├── Data/                # Database context and initialization
├── Migrations/          # Entity Framework migrations
├── Properties/          # Launch settings
└── Program.cs           # Application entry point
```

### Frontend Project Structure

```
inventory-frontend/
├── src/
│   ├── app/
│   │   ├── components/      # Angular components
│   │   ├── guards/          # Route guards
│   │   ├── models/          # TypeScript interfaces
│   │   ├── services/        # API services
│   │   └── app.routes.ts    # Route definitions
│   ├── environments/        # Environment configs
│   └── styles.css          # Global styles
└── angular.json            # Angular configuration
```

### Key Backend Files

- `Models/Equipment.cs` - Equipment entity and enums
- `Models/DBContext.cs` - Entity Framework context
- `Controllers/EquipmentController.cs` - Equipment API endpoints
- `Data/DbInit.cs` - Database seeding logic

### Key Frontend Files

- `app/services/auth.service.ts` - Authentication service
- `app/services/equipment.service.ts` - Equipment API service
- `app/guards/auth.guard.ts` - Route protection
- `app/app.routes.ts` - Application routing

## 📱 Frontend Features Guide

### User Workflow
1. Register or login to the system
2. Browse equipment catalog with search/filter
3. Request available equipment
4. Track request status
5. View borrowing history

### Admin Workflow
1. Login with admin credentials
2. Manage equipment inventory (CRUD operations)
3. Review and approve/reject user requests
4. Track equipment returns and condition
5. Generate and export reports
6. View usage analytics

## 🎨 UI Features

- Responsive design for desktop and tablet
- Role-based navigation menu
- Interactive dashboards with statistics
- Real-time status updates
- Equipment photo display
- Filterable and searchable tables
- Export functionality for reports

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