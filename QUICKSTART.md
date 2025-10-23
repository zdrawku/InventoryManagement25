# Quick Start Guide - Inventory Management 2025

## Current Status

✅ **Frontend (Angular)**: Fully implemented and ready
⚠️ **Backend (ASP.NET Core)**: Partially implemented

### What's Working
- Equipment browsing (read-only via existing Equipment API)
- Frontend UI for all features (Login, Register, Dashboard, Equipment, Requests, Reports)
- Frontend routing and navigation
- Role-based UI components

### What Needs Backend Support
- User authentication and registration
- Equipment request management
- Report generation
- Admin approval workflows

## Quick Start (Current State)

### 1. Start Backend API

```bash
cd InventoryManagement2025
dotnet run --launch-profile https
```

Backend runs at: https://localhost:7122
Swagger docs: https://localhost:7122/swagger

### 2. Start Frontend

```bash
cd inventory-frontend
npm install
npm start
```

Frontend runs at: http://localhost:4200

### 3. Browse Equipment (Working Feature)

1. Open http://localhost:4200
2. Click "Equipment" in the navigation (works without login)
3. Browse existing equipment from the database
4. Use search and filter features

## Full Setup (When Backend is Complete)

Once all backend endpoints are implemented (see BACKEND_REQUIREMENTS.md):

### 1. Create Admin User
Register via POST /api/auth/register or seed database with admin user

### 2. Login
Navigate to http://localhost:4200/login

### 3. Test Features

**As User:**
- Browse equipment catalog
- Request equipment
- View request status
- Track borrowing history

**As Admin:**
- All user features
- Add/Edit/Delete equipment
- Approve/Reject requests
- Mark equipment as returned
- View reports and analytics

## Testing the Frontend

The frontend can be tested independently using the browser:

1. **UI/UX Testing**: Navigate through all pages to verify layouts
2. **Form Validation**: Try submitting forms with empty/invalid data
3. **Routing**: Test navigation between pages
4. **Responsive Design**: Resize browser window to test responsiveness

### Manual Testing Checklist

- [ ] Login page loads and displays correctly
- [ ] Register page loads and displays correctly  
- [ ] Equipment list page shows sample equipment (from existing API)
- [ ] Equipment detail page works for existing equipment
- [ ] Search and filter controls work on equipment list
- [ ] Navigation menu shows/hides based on mock authentication state
- [ ] Dashboard layout displays correctly
- [ ] Request form displays correctly
- [ ] Reports page displays correctly
- [ ] All buttons and links are properly styled

## Development Workflow

### Frontend Development

```bash
cd inventory-frontend

# Install dependencies
npm install

# Run development server
npm start

# Build for production
npm run build

# Run tests
npm test
```

### Backend Development

```bash
cd InventoryManagement2025

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update

# Run in development
dotnet run

# Build for production
dotnet build --configuration Release
```

## Environment Variables

### Frontend (`inventory-frontend/src/environments/`)

**environment.ts** (Development)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5220/api'  // or https://localhost:7122/api
};
```

**environment.prod.ts** (Production)
```typescript
export const environment = {
  production: true,
  apiUrl: '/api'  // Use relative URL in production
};
```

## Troubleshooting

### CORS Errors
If you see CORS errors in the browser console:
- Ensure backend CORS policy includes `http://localhost:4200`
- Check backend Program.cs for CORS configuration

### API Connection Failed
- Verify backend is running on expected port
- Check `apiUrl` in frontend environment files
- Verify CORS is configured correctly

### Authentication Not Working
This is expected - authentication endpoints need to be implemented in the backend first. See BACKEND_REQUIREMENTS.md.

### Equipment List Empty
- Verify backend database has sample data
- Check backend is running and accessible
- Open browser dev tools to see API errors

## Next Steps

1. **Review Backend Requirements**: See BACKEND_REQUIREMENTS.md
2. **Implement Missing Backend APIs**: Authentication, Requests, Reports
3. **Test Integration**: Verify frontend and backend work together
4. **Deploy**: Configure for production environment

## Support

For issues or questions:
- Check browser console for frontend errors
- Check backend console for API errors
- Review Swagger docs at https://localhost:7122/swagger
- See README.md and FRONTEND_README.md for detailed documentation
