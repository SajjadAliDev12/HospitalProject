# ARCHITECTURE.md
### Hospital Management System — authoritative technical blueprint
*Derived 2026-09-20 directly from the repository source (solution `HospitalProject.slnx`, 3 projects). Every statement below is traceable to actual code in this repo; nothing is inferred or extrapolated.*

---

## 1. Architectural Style

- **Layered Client→Server monolith (3 physical projects, 2 tiers):**
  1. `Hospital.Core` — **shared layer** (domain models, DTOs, enums). No EF, no HTTP, no Serilog configuration (Serilog package referenced from Core's perspective only for the shared Serilog namespace in one DTO? no — Serilog.AspNetCore is a package reference in every project because the toolchain needs `Microsoft.Extensions.Identity.Stores`; logging is configured only in `Hospital.API/Program.cs`).
  2. `Hospital.API` — **back-end tier**: ASP.NET Core Web API (net8.0), EF Core 8 + SQL Server, ASP.NET Identity + JWT, Serilog, Swagger. No repository/unit-of-work/business-layer abstraction — controllers speak directly to `ApplicationDbContext`.
  3. `Hospital.Desktop` — **front-end tier**: WPF (net8.0-windows) desktop client using hand-rolled **MVVM** (`BaseViewModel`/`RelayCommand` — `CommunityToolkit.Mvvm` package is referenced (`4.8.1`) but not used).
- **Patterns in use:** MVVM (Desktop), DI via constructors (API controllers/context/services), direct `ApplicationDbContext` (no repository), Arabic-first UI/messages.
- **Core tenets:** thin controllers; all data access via `ApplicationDbContext`; soft-delete (`isDeleted`) is a first-class convention on every domain entity; global `Authorize` by default; Arabic-first domain (UI + messages + reports).

---

## 2. Solution Structure & Project Boundaries

### 2.1 Hospital.Core (class library)
- **Contents:** `Models/` (10 entities): `Employee`, `Department`, `JobTitle` (entity uses typo `JobTitleVeiw`? — actually `JobTitle.cs` class `JobTitle`), `Leave`, `Absent`, `NightShiftTeam`, `TransferLog`, `AuditLog`, `SystemSetting`, `ApplicationUser`; `DTOs/` (15 DTO files: `CreateEmployeeDTO`, `EmployeeFullDTO`, `EmployeeSimpleDTO`, `EmployeeLookupDto`, `PagedResult<T>`, `LoginDto`, `RegisterDTO`, `UserFormDTO`, `DepartmentDto`, `CreateDepartmentDto`, `JobTitleDTO` + `JobTitleViewDTO` (renamed from `JobTitleVeiwDTO` 2026-09-20), `LeaveDTO`, `AbsentDTO`, `TransferLogDto`, `SystemSettingDto`, `ChangePasswordDto`, `AdminResetDto`); `Enums/Enums.cs` (8 enums: `enGender`, `enShiftType`, `enJobStatus`, `enLeaveType`, `enCertificate`, `enRole`, `enAuditType`, `enMorningShifts`).
- **Responsibility:** shared contracts. **Does not:** make HTTP calls, touch DB, reference the other two projects. **Consumers:** API, Desktop.
- **Notable:** entity/Arabic comments; enum variants mixed-case/pascal (`enGender.Female`); DTO casing inconsistent — keep as-is.

### 2.2 Hospital.API (Web API; startup project)
- **Contents:** `Controllers/` (10 controllers), `Data/` (`ApplicationDbContext`, `DbSeeder`), `Middleware/` (`ExceptionMiddleware`), `Migrations/` (9 migrations + `ApplicationDbContextModelSnapshot.cs`), `Services/ShiftService.cs`, `Program.cs`, `appsettings*.json` (gitignored — local secrets), `Properties/launchSettings.json`.
- **Responsibility:** REST surface. **Does:** `Program.cs` — ASP.NET Identity + JWT validation, Serilog, Swagger (JWT Bearer auth), CORS, `DbSeeder.SeedRolesAndAdminAsync` at startup.
- **Notable:** all controllers require JWT unless `[AllowAnonymous]`; controllers use `[Authorize(Roles=…)]` per-action.

### 2.3 Hospital.Desktop (WPF MVVM client)
- **Contents:** `App.xaml` (StartupUri: `Views/LoginView.xaml`), `MainWindow.xaml(.cs)` (RTL shell + sidebar + ContentControl `CurrentView`), `Views/` (21 XAML views, incl. `ReportPreviewWindow`), `ViewModels/` (per-view VMs), `Converters/StatusConverters.cs` + other converters, `Services/ApiService.cs` + `EncryptionHelper.cs` + `ReportGenerator.cs`, `Properties/` (Settings `SavedUsername`/`SavedPassword` encrypted via DPAPI + `IsRemembered`).
- **Responsibility:** desktop UI for hospital staff. **Does:** login (JWT → in-memory token), CRUD via API, local shift-table computation (`ShiftService`/`ReportGenerator`), print via WPF `PrintDialog`.
- **Notable:** **exactly one** `Services/ApiService.cs` (no duplicate, no `ApiClient.cs`); base URL hardcoded `https://localhost:7278/api/`; no DI container — VMs `new ApiService()` themselves; `CommunityToolkit.Mvvm` referenced but **not used**.

---

## 3. AuthN/AuthZ & Security

### 3.1 Identity + JWT chain (API — `Program.cs`)
- `AddIdentity<ApplicationUser, IdentityRole>()` → `AddEntityFrameworkStores<ApplicationDbContext>()` → `AddDefaultTokenProviders()`.
- `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` + `AddJwtBearer`; `TokenValidationParameters`: `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey`; `ValidIssuer`/`ValidAudience` from `Jwt` section; `IssuerSigningKey = SymmetricSecurityKey(utf8(Jwt:Key))`; `RoleClaimType = ClaimTypes.Role`.
- `AddControllers` with a global `AuthorizeFilter` (`RequireAuthenticatedUser`) — **every `[ApiController]` action requires a valid JWT unless `[AllowAnonymous]`**.
- Controllers opt in per-role: e.g., `EmployeesController` methods `[Authorize(Roles = "Admin,Manager")]` / `[Authorize(Roles = "Admin")]`; `AuthController.GetUsers`/`UpdateUser`/`DeleteUser` are `[Authorize(Roles = "Admin")]`; `LeavesController` writes `Admin,Manager`.
- **Roles seeded (DbSeeder):** `Admin`, `Manager`, `User`.

### 3.2 Auth endpoints (`Hospital.API/Controllers/AuthController.cs`)
- `POST api/Auth/Login` — `[AllowAnonymous]`, body `LoginDto {UserName, Password}`; builds claims `ClaimTypes.Name` (UserName), `ClaimTypes.NameIdentifier` (user.Id), `Jti` (guid), `FullName`, + one `ClaimTypes.Role` per user role; `expires` from `Jwt:DurationInDays` (default 7); returns `{token, expiration, username}`; on failure → `401` with Arabic message `اسم المستخدم أو كلمة المرور غير صحيحة`.
- `POST api/Auth/Register` — `[Authorize(Roles = "Admin")]`, `RegisterDTO {UserName, Password, FullName, EmployeeId}` → creates user linked to `EmployeeId`, adds to role `User`.
- `GET api/Auth/Users` — `[Authorize(Roles = "Admin")]`; filters `IsDeleted`/`IsActive`; joins roles, includes `EmployeeId`, `FullName`.
- `PUT api/Auth/UpdateUser` — `[Authorize(Roles = "Admin")]`; updates user props + role.
- `DELETE api/Auth/{id}` — `[Authorize(Roles = "Admin")]` → soft delete (`IsDeleted=true`, `IsActive=false`).
- `POST api/Auth/ChangePassword` — change own password.
- `POST api/Auth/AdminResetPassword` — `[Authorize(Roles = "Admin")]`; resets another user's password via token.

### 3.3 Claims & role model
- `ApplicationUser : IdentityUser` fields: `FullName`, `EmployeeId`, `IsDeleted`, `IsActive`, `Employee` nav.
- `Login` only checks `IsDeleted`/`IsActive` at login time — **soft-deleted users keep a valid token until expiry** (no per-request check in middleware).

### 3.4 Secrets & config
- `appsettings.json` / `appsettings.Development.json` are **gitignored** (contain `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:DurationInDays`, `ConnectionStrings:DefaultConnection`). **Never commit them.**
- JWT config keys used in code: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:DurationInDays`.

---

## 4. Database Architecture

- **Provider/ORM:** SQL Server (LocalDB via `DefaultConnection`), EF Core 8.0.10 (`Microsoft.EntityFrameworkCore.SqlServer`), migrations in `Hospital.API/Migrations/` (9 migrations + snapshot).
- **DbContext:** `Hospital.API/Data/ApplicationDbContext : IdentityDbContext<ApplicationUser>`, with:
  - soft-delete query filters (`HasQueryFilter(e => !e.isDeleted)`) on `Employee`/`Department`/`Leave`/`Absent`; `IgnoreQueryFilters()` in controllers to read deleted rows.
  - cascades: `Employee→Leaves`/`Absents` cascade; department/manager/transfer uses `Restrict`/`NoAction`.
  - `OnModelCreating` seeds 4 `NightShiftTeam` rows (`Id 1..4`).
- **Tables (from `Hospital.sql` + migrations):** `Employees` (DepartmentId, JobTitleId, LeaveCardNumber, LeaveBalance, ShiftType, BirthDate, HireDate, Gender, CertificateType, JobStatus, Address, PhoneNumber, NightShiftId, enMorningShifts, isDeleted), `Departments` (Name, ManagerId, ManagerOrderNumber, ManagerStartDate, isDeleted), `JobTitles`, `Leaves`, `Absents` (EmployeeId, Date), `NightShiftTeams`, `TransferLogs`, `SystemSettings` (ShiftReferenceDate), `AuditLogs`, `AspNetUsers`/Identity tables.
- **Migrations (9, oldest→newest):** `InitialCreateV2`, `UpdateApplicationUser`, `editAuditLog`, `editAuditLogUserID`, `LeaveCardNumber`, `updateEmployee`, `updateDepartments`, `addsystemsetting`, `newfeature` (current `20260402181244_newfeature`). Snapshot kept current.

---

## 5. API Surface (Routing Reference)

All routes under `api/`, JWT required unless noted `[AllowAnonymous]`.

| Controller | Route prefix | Key actions |
|---|---|---|
| AuthController | `api/Auth` | `Login`(anon), `Register`(Admin), `Users`(Admin), `UpdateUser`(Admin), `ChangePassword`, `AdminResetPassword`(Admin), `DeleteUser`(Admin) |
| EmployeesController | `api/Employees` | `GET` (paged + search), `GET {id}`, `GET report-data` (report), `POST`, `PUT {id}`, `DELETE {id}`, `GET Search` |
| DepartmentsController | `api/Departments` | `GET`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}` |
| JobTitlesController | `api/JobTitles` | `GET`, `GET {id}?`, `POST`, `PUT {id}`, `DELETE {id}` |
| LeavesController | `api/Leaves` | `GET` (paged), `POST`, `PUT {id}`, `DELETE {id}` |
| AbsentsController | `api/Absents` | `GET` (paged), `POST`, `PUT {id}`, `DELETE {id}` |
| TransferLogController | `api/TransferLog` | `GET` (paged, search), `POST`, `PUT {id}`? |
| AuditLogsController | `api/AuditLogs` | `GET` (paged) — Admin-only |
| NightShiftTeamsController | `api/NightShiftTeams` | `GET`, `POST`, `PUT {id}` |
| ShiftsController | `api/Shifts` | `GET calculate` (team-by-date), `POST`, `PUT {id}`, `GET report-data` |

---

## 6. Cross-Cutting Concerns

### 6.1 Exception handling
- `Hospital.API/Middleware/ExceptionMiddleware.cs` — global try/catch on the HTTP pipeline, logs via Serilog, returns Arabic JSON `{message}` + HTTP 500. Registered via `app.UseMiddleware<ExceptionMiddleware>()` in `Program.cs`.
- Controllers also wrap POST/PUT in try/catch and return Arabic `{message}`.

### 6.2 Validation
- API: `[ApiController]` automatic model-state (400 with Arabic messages via DataAnnotations); plus manual checks in controllers (`if (!await …AnyAsync()) return BadRequest`).
- Desktop: `BaseViewModel` + `RelayCommand` can-execute gates; manual checks in VMs.

### 6.3 Logging (Serilog)
- API: `.WriteTo.Console()` + `.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Month)` in `Program.cs`; `UseSerilog`.
- Desktop: no Serilog init observed (`Serilog.AspNetCore` referenced in csproj).

### 6.4 Audit trail
- `AuditLogs` table + `AuditLogsController` (read-only, paged). Writes in `ApplicationDbContext.SaveChangesAsync` override — captures `Added/Modified/Deleted` entries, derives `enAuditType` (Transfer/Leave/Absent/Add/Edit/Delete), writes `AuditLog` rows (userId from `NameIdentifier` claim via `IHttpContextAccessor`); saves in two phases to get real IDs.

### 6.5 Shifts (night-shift rotation)
- `Hospital.API/Services/ShiftService.cs` implements `IShiftService`:
  - `GetTeamIdByDate(DateOnly)` — days-diff from `SystemSettings.ShiftReferenceDate` modulo 4, mapped to team `1..4` (morning groups handled by `enMorningShifts`).
  - `GetCurrentShiftDetail(DateOnly)` — loads `NightShiftTeam` with `Supervisor`.
- Used by `ShiftsController` for team-by-date calculation.

---

## 7. Desktop MVVM Flow (end-to-end)

```
LoginView.xaml (LoginViewModel) --POST api/Auth/Login--> ApiService.PostAsync --> AuthController.Login --> JWT -> ApiService.SetToken
MainWindow.xaml (MainViewModel)
   ├── Sidebar (RadioButtons: Employees, Departments, Leaves, Absents, Users, JobTitles, Transfers, NightShiftTeams, AuditLogs, Shifts)
   └── ContentControl CurrentView (DataTemplates map VM→View)
EmployeesViewModel -> LoadEmployees() -> GET api/Employees/search?...   (paging PageSize=?)
   -> DataGrid binding + filtering
EmployeeForm (EmployeeFormViewModel) -> POST/PUT api/Employees
LeavesViewModel -> GET api/Leaves + filters -> POST/PUT api/Leaves
ReportGenerator (ReportPreviewWindow) -> builds FlowDocument report (RTL Arabic), PrintDialog
Logout -> back to LoginView
```

**Navigation:** `MainViewModel.NavCommand` switches `CurrentView`; each VM loads data in `LoadXxx()` (async); `ObservableCollection<T>` + `ICollectionView` + paging.

---

## 8. Known Deviations / Tech-Debt (read before changing)

These are **intentional/current-code facts**, not prescribed changes:

1. **`RelayCommand`/`BaseViewModel` are hand-rolled MVVM** — `CommunityToolkit.Mvvm` is referenced (8.x) but never used. Don't assume toolkit availability; follow the existing `BaseViewModel`/`RelayCommand` pattern (or the `RelayCommand` inside `LoginViewModel.cs`).
2. **Controllers use `ApplicationDbContext` directly** — no repository/unit-of-work. New CRUD should follow existing controllers (inject `ApplicationDbContext`).
3. **Soft-delete is pervasive** (`isDeleted` on every domain entity) — always honor `IgnoreQueryFilters()` to include/exclude deleted rows explicitly.
4. **Date-only handling:** entities use `DateOnly` for BirthDate/HireDate/ShiftReferenceDate; EF stores as `date`; Desktop uses a `DateOnlyJsonConverter` in its JSON settings for GETs.
5. **Inconsistent DTO casing (kept as-is):** mixed `DTO`/`Dto` suffix, `LeaveCardNumber` vs `leaveCardNumber`, `DateOnlyJsonConverter` in Desktop `Converters`. (Historical: `JobTitleVeiwDTO` fixed to `JobTitleViewDTO` on 2026-09-20; no `JobTitleVeiw` entity ever existed - earlier claim was a doc hallucination.)
6. **RTL / Arabic-first:** new strings/messages/labels must be Arabic; RTL layout per-view.
7. **Enum naming (kept as-is):** `enGender.Female`, `enRole.{Admin,Manager,User}`, `enMorningShifts.{SaturdayGroup,ThursdayGroup}`, `enCertificate.{HighSchool,…}`.
8. **Serilog only in API** — Desktop doesn't initialize Serilog despite the package reference.
9. **CORS:** open (allow any) in `Program.cs`.

---

## 9. Ports & Local Dev

- API launch: `https://localhost:7278` (https) / `http://localhost:5180` (http) — see `Properties/launchSettings.json`.
- Desktop `ApiService` hardcodes base URL `https://localhost:7278/api/`.
- `Hospital.API.http` (REST Client) references `https://localhost:7278/api/...`.

---

## 10. Verify / Build

```bash
# Restore + build (slnx)
dotnet build HospitalProject.slnx

# Migrations (from API dir)
cd Hospital.API
dotnet ef migrations add <Name>
dotnet ef database update

# Run API
dotnet run --project Hospital.API

# Run Desktop
dotnet run --project Hospital.Desktop
```
