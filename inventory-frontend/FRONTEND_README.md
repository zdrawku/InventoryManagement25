# Inventory Management Frontend

Angular 20 frontend application for the Inventory Management System.

## Features

### User Roles
- **Users**: Can view equipment, request items, and track their borrowing history
- **Administrators**: Full access including equipment management, request approvals, and reports

### Main Capabilities

#### Authentication & Authorization
- User registration and login
- Role-based access control (User, Admin)
- Protected routes with guards

#### Equipment Management
- Browse equipment catalog with search and filtering
- View equipment details including:
  - Name, type, serial number
  - Condition (Excellent, Good, Fair, Damaged)
  - Status (Available, Checked Out, Under Repair, Retired)
  - Location and photo
- Admin-only features:
  - Add new equipment
  - Edit equipment details
  - Delete equipment
  - Update equipment status

#### Request System
- Users can request available equipment
- Specify request date and needed-until date
- Track request status (Pending, Approved, Rejected, Returned)
- Admin features:
  - View all requests
  - Approve/reject requests
  - Mark equipment as returned
  - Log equipment condition on return

#### Reports & Analytics
- Usage reports showing equipment utilization
- History logs of all equipment transactions
- Export reports in CSV/PDF format

#### Dashboard
- Personalized dashboard for Users and Admins
- Quick stats overview
- Recent activity
- Quick action buttons

## Technology Stack

- **Framework**: Angular 20
- **Language**: TypeScript 5.9
- **Styling**: CSS
- **HTTP Client**: Angular HttpClient
- **Routing**: Angular Router
- **State Management**: Angular Signals

## Prerequisites

- Node.js 20.x or higher
- npm 10.x or higher
- Angular CLI 20.x

## Installation

1. Navigate to the frontend directory:
```bash
cd inventory-frontend
```

2. Install dependencies:
```bash
npm install
```

## Configuration

The application uses environment files for configuration:

- `src/environments/environment.ts` - Development environment
- `src/environments/environment.prod.ts` - Production environment

Update the `apiUrl` in these files to point to your backend API:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5220/api'  // Update this URL
};
```

## Development Server

Run the development server:

```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any source files.

## Build

Build the project for production:

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Running Tests

Execute unit tests:

```bash
npm test
```

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── auth/              # Login and registration
│   │   ├── dashboard/         # Dashboard components
│   │   ├── equipment/         # Equipment management
│   │   ├── requests/          # Request management
│   │   ├── reports/           # Reports and analytics
│   │   └── shared/            # Shared components (navbar, etc.)
│   ├── guards/
│   │   └── auth.guard.ts      # Route guards
│   ├── models/                # TypeScript interfaces
│   ├── services/              # API services
│   ├── app.config.ts          # App configuration
│   ├── app.routes.ts          # Route definitions
│   └── app.ts                 # Root component
├── environments/              # Environment configurations
└── styles.css                 # Global styles
```

## API Integration

The frontend communicates with the backend REST API with the following endpoints:

### Authentication
- POST `/auth/register` - User registration
- POST `/auth/login` - User login
- GET `/auth/logout` - User logout

### Equipment
- GET `/equipment` - Get all equipment
- GET `/equipment/{id}` - Get equipment by ID
- POST `/equipment` - Create equipment (Admin only)
- PUT `/equipment/{id}` - Update equipment (Admin only)
- PUT `/equipment/{id}/status` - Update status (Admin only)
- DELETE `/equipment/{id}` - Delete equipment (Admin only)

### Requests
- POST `/request` - Create request
- GET `/requests` - Get user's requests
- GET `/manager/requests` - Get all requests (Admin only)
- PUT `/request/{id}/approve` - Approve request (Admin only)
- PUT `/request/{id}/reject` - Reject request (Admin only)
- PUT `/request/{id}/return` - Return equipment (Admin only)

### Reports
- GET `/reports/usage` - Usage report (Admin only)
- GET `/reports/history` - History logs (Admin only)
- GET `/reports/export` - Export reports (Admin only)

## User Guide

### For Users

1. **Register/Login**: Create an account or login with existing credentials
2. **Browse Equipment**: View available equipment in the catalog
3. **Search & Filter**: Use filters to find specific equipment
4. **Request Equipment**: Click "Request" on available items
5. **Track Requests**: View your request status in "My Requests"

### For Administrators

All User features plus:

1. **Manage Equipment**:
   - Add new equipment via "Add Equipment" button
   - Edit or delete existing equipment
   - Update equipment status

2. **Manage Requests**:
   - View all user requests
   - Approve or reject pending requests
   - Mark equipment as returned
   - Log condition on return

3. **View Reports**:
   - Access usage statistics
   - Review history logs
   - Export reports in CSV/PDF

## Troubleshooting

### CORS Issues
If you encounter CORS errors, ensure the backend API has CORS configured to allow requests from `http://localhost:4200`.

### API Connection
Verify the `apiUrl` in environment files matches your backend API URL.

### Authentication Issues
Clear browser local storage if experiencing login issues:
```javascript
localStorage.clear();
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit a pull request

## License

This project is part of the Inventory Management System 2025.
