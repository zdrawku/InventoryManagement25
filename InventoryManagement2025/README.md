# InventoryManagement2025 - Quick Notes

This project includes role-based identity, equipment request workflow, condition logging, and small admin export endpoints.

New endpoints
- POST /api/auth/register — register a user (if you kept the minimal endpoints)
- POST /api/auth/login — login and receive a JWT
- GET /api/Equipment — list equipment (authenticated)
- GET /api/Equipment/search?name=&type=&status=&condition= — search equipment (authenticated)
- GET /api/Equipment/export/csv — admin-only, export equipment CSV
- GET /api/Equipment/export/requests/csv — admin-only, export requests CSV
- POST /api/EquipmentRequests — create a request (authenticated)
- GET /api/EquipmentRequests/mine — list my requests
- GET /api/EquipmentRequests — admin-only, list all requests
- PATCH /api/EquipmentRequests/{id}/approve — admin-only
- PATCH /api/EquipmentRequests/{id}/reject — admin-only
- PATCH /api/EquipmentRequests/{id}/return — admin-only, mark returned and create a ConditionLog

Seeded admin
- By default Program.cs seeds an admin user with email `admin@school.local` and password `Admin123!` if not present. Change these in configuration for production.

Local dev commands (PowerShell)
Restore and build:
```powershell
cd 'c:\VsC projects\InventoryManagement25\InventoryManagement2025'
dotnet restore
dotnet build
```

Create/apply migrations (if you want EF to generate them) or apply the included migration file:
```powershell
# Option A (preferred if you want EF to scaffold the migration):
dotnet ef migrations add AddRequestFieldsAndLogs
dotnet ef database update

# Option B (if you keep the manual migration file added to the repo):
dotnet ef database update
```

Run the app:
```powershell
dotnet run
```

Run tests:
```powershell
cd '..\InventoryManagement2025.Tests'
dotnet test
```

Notes
- The repository includes a manually-added migration `Migrations/20251015170000_AddRequestFieldsAndLogs.cs` which adds the `ConditionLogs` table and request return fields if EF couldn't scaffold the migration in your environment.
- CSV export endpoints return a UTF-8 CSV with simple quoting; if you expect complex values, consider improving CSV escaping.
