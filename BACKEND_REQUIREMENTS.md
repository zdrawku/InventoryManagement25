# Backend API Requirements

The Angular frontend application expects the following REST API endpoints to be implemented in the backend. Currently, only the Equipment endpoints are implemented.

## ✅ Implemented Endpoints

### Equipment Management
- `GET /api/equipment` - Get all equipment
- `GET /api/equipment/{id}` - Get equipment by ID  
- `POST /api/equipment` - Create equipment
- `PUT /api/equipment/{id}` - Update equipment
- `DELETE /api/equipment/{id}` - Delete equipment

## ⏳ Required Endpoints (Not Yet Implemented)

### Authentication
These endpoints need to be added to support user authentication and authorization:

- `POST /api/auth/register` - User registration
  - Request: `{ username, email, password, role }`
  - Response: `{ token, user: { id, username, email, role } }`
  
- `POST /api/auth/login` - User login
  - Request: `{ username, password }`
  - Response: `{ token, user: { id, username, email, role } }`
  
- `GET /api/auth/logout` - User logout
  - Response: Success message

### Equipment Status Update
- `PUT /api/equipment/{id}/status` - Update equipment status only
  - Request: `{ status }`
  - Response: Success/No Content

### Request Management
- `POST /api/request` - Create equipment request
  - Request: `{ equipmentId, requestDate, requestedUntil, notes }`
  - Response: Created request object
  
- `GET /api/requests` - Get current user's requests
  - Response: Array of user's requests
  
- `GET /api/manager/requests` - Get all requests (Admin only)
  - Response: Array of all requests
  
- `PUT /api/request/{id}/approve` - Approve request (Admin only)
  - Response: Success/No Content
  
- `PUT /api/request/{id}/reject` - Reject request (Admin only)
  - Response: Success/No Content
  
- `PUT /api/request/{id}/return` - Mark equipment as returned (Admin only)
  - Request: `{ condition, notes }`
  - Response: Success/No Content

### Reports
- `GET /api/reports/usage` - Get usage report (Admin only)
  - Response: Array of usage statistics
  
- `GET /api/reports/history` - Get history logs (Admin only)
  - Query params: `equipmentId`, `userId` (optional filters)
  - Response: Array of history log entries
  
- `GET /api/reports/export` - Export reports (Admin only)
  - Query params: `format` (csv|pdf), `reportType` (usage|history), `startDate`, `endDate`
  - Response: File download (Blob)

## Required Models

### User Model
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}

public enum UserRole
{
    User,
    Admin
}
```

### Request Model
```csharp
public class EquipmentRequest
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public int UserId { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime RequestedUntil { get; set; }
    public RequestStatus Status { get; set; }
    public string Notes { get; set; }
    
    // Navigation properties
    public Equipment Equipment { get; set; }
    public User User { get; set; }
}

public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    Returned
}
```

### Report Models
```csharp
public class UsageReport
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; }
    public int TotalRequests { get; set; }
    public int TotalDays { get; set; }
    public string CurrentStatus { get; set; }
}

public class HistoryLog
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string Notes { get; set; }
}
```

## Implementation Notes

1. **Authentication**: Consider using JWT tokens for authentication
2. **Authorization**: Implement role-based authorization using [Authorize] attributes
3. **CORS**: Already configured in Program.cs - ensure it allows the frontend origin
4. **Database**: Add migrations for new models (User, EquipmentRequest, HistoryLog)
5. **Password Hashing**: Use secure password hashing (e.g., BCrypt or ASP.NET Identity)
6. **Error Handling**: Implement proper error responses (400, 401, 403, 404, 500)

## Frontend Impact

The frontend is fully implemented and expects these endpoints. Until the backend endpoints are implemented:
- Authentication features will not work
- Request management will not work  
- Reports will not work
- Only equipment browsing (read-only) will function

## Next Steps for Backend Development

1. Install necessary NuGet packages:
   - `Microsoft.AspNetCore.Authentication.JwtBearer`
   - `BCrypt.Net-Next` (for password hashing)

2. Create new models (User, EquipmentRequest, HistoryLog)

3. Update DbContext to include new DbSets

4. Create and run EF migrations

5. Implement controllers:
   - AuthController
   - RequestController  
   - ReportsController

6. Add authentication middleware and JWT configuration

7. Add authorization policies for Admin-only endpoints

8. Test all endpoints with Swagger

9. Update API documentation
