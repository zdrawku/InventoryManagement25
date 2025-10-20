# Inventory Management Data Model

The application tracks school equipment, lending activity, and equipment condition history. The following tables compose the relational model. Enum-backed fields are stored as readable strings through EF Core value conversions, and this document is kept current alongside the EF Core models.

## Entities

### User
| Column | Type | Notes |
| --- | --- | --- |
| `UserId` | `INTEGER` | Primary key, identity |
| `FirstName` | `TEXT` | Required, max 50 chars |
| `LastName` | `TEXT` | Required, max 50 chars |
| `Email` | `TEXT` | Required, max 100 chars, unique |
| `PasswordHash` | `TEXT` | Required, max 500 chars, salted hash |
| `PhoneNumber` | `TEXT` | Optional, max 20 chars |
| `Role` | `TEXT` | Enum: Administrator, Staff, Student, Technician |
| `Department` | `TEXT` | Optional, max 100 chars |
| `IsActive` | `INTEGER` | Boolean flag, defaults to true |
| `CreatedAt` | `TEXT` | UTC timestamp, defaults to current timestamp |
| `PasswordChangedAt` | `TEXT` | Optional UTC timestamp for auditing |
| `LastLoginAt` | `TEXT` | Optional UTC timestamp of most recent login |

### Equipment
| Column | Type | Notes |
| --- | --- | --- |
| `EquipmentId` | `INTEGER` | Primary key, identity |
| `Name` | `TEXT` | Required, max 100 chars |
| `Type` | `TEXT` | Optional, max 50 chars |
| `SerialNumber` | `TEXT` | Required, max 100 chars, unique |
| `Condition` | `TEXT` | Enum: Excellent, Good, Fair, Damaged |
| `Status` | `TEXT` | Enum: Available, CheckedOut, Maintenance, Retired |
| `Location` | `TEXT` | Optional, max 100 chars |
| `PhotoUrl` | `TEXT` | Optional, max 2048 chars |

### Request
| Column | Type | Notes |
| --- | --- | --- |
| `RequestId` | `INTEGER` | Primary key, identity |
| `EquipmentId` | `INTEGER` | FK → `Equipment(EquipmentId)` |
| `UserId` | `INTEGER` | FK → `User(UserId)` |
| `Type` | `TEXT` | Enum: Checkout, Return, Maintenance |
| `Status` | `TEXT` | Enum: Pending, Approved, Denied, Completed, Cancelled |
| `RequestedAt` | `TEXT` | UTC timestamp, defaults to current timestamp |
| `NeededBy` | `TEXT` | Optional deadline timestamp |
| `CompletedAt` | `TEXT` | Optional completion timestamp |
| `Notes` | `TEXT` | Optional, max 500 chars |

### ConditionLog
| Column | Type | Notes |
| --- | --- | --- |
| `ConditionLogId` | `INTEGER` | Primary key, identity |
| `EquipmentId` | `INTEGER` | FK → `Equipment(EquipmentId)` |
| `LoggedByUserId` | `INTEGER` | Nullable FK → `User(UserId)` |
| `Condition` | `TEXT` | Enum: Excellent, Good, Fair, Damaged |
| `LoggedAt` | `TEXT` | UTC timestamp, defaults to current timestamp |
| `Notes` | `TEXT` | Optional, max 500 chars |

## Relationships
- **User → Request**: One-to-many. Deleting a user is restricted if requests exist.
- **Equipment → Request**: One-to-many. Deleting equipment is restricted if requests exist.
- **Equipment → ConditionLog**: One-to-many with cascade delete to remove historical logs when equipment is deleted.
- **User → ConditionLog**: Optional one-to-many with set-null delete behavior.

These constraints are enforced through the Entity Framework Core model configuration and the generated SQLite schema.
