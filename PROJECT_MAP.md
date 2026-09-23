# PROJECT_MAP.md — Hospital Management System (HospitalProject)

> **Repo:** `C:\Users\scorp\source\repos\HospitalProject`
> **Map maintained for:** agents/LLMs/CLI users who must find, understand, and modify the codebase without guessing.
> Every path below was verified against the actual source tree. If a file is mentioned, it exists.

---

## 1. Repo Layout (verified tree)

```
HospitalProject/                                  WORKSPACE ROOT (git repo `HospitalProject`)
├── HospitalProject.slnx                     # XML solution (new .slnx format) — 3 projects, no classic .sln
├── HospitalProject.slnLaunch.user           # (personal, local) dev launch profile combining API + Desktop
├── PROJECT_MAP.md                           # THIS FILE — navigational map
├── all_classes.txt                          # Concatenated scratch dump of API + Desktop class files (repository artifact)
├── Hospital.sql                             # Scripted DB export (SQL Server, database: HospitalManagementDB)
├── .gitignore                               # Excludes bin/, obj/, .vs/, *.user, appsettings* (secrets), Logs/, *.sql, DB dumps
├── .github/workflows/build.yml            # EXISTS — CI builds `HospitalProject.slnx` on windows-latest (added 2026-09-20, TASK-INF-04)
├── README.md / LICENSE (MIT) / .editorconfig # EXIST (added 2026-09-20, TASK-INF-01..03)
├── Hospital.Core/                           # Class library — shared domain layer (no EF, no HTTP, no Identity beyond ASP.NET Identity stores)
├── Hospital.API/                            # Web API (ASP.NET Core, net8.0) — backend; startup project
└── Hospital.Desktop/                        # WPF (net8.0-windows) — MVVM desktop client; WinExe
```

> **Solution format note:** only `.slnx` (XML) is present. There is **no** classic `.sln`, no test project, no Dockerfile. `dotnet build HospitalProject.slnx` is the canonical build entry (2026-09-21: **0 errors, 110 warnings** — down from ~258 baseline after TASK-QA-01 Pass A). `README.md`, `LICENSE` (MIT), `.editorconfig`, CI (`build.yml`) exist since 2026-09-20.

---

## 2. Project-by-Project Breakdown

### 2.1 `Hospital.Core/` (negligible dependencies — shared contracts)
| Path | Contents |
|---|---|
| `Models/` | 10 domain entities: `Employee`, `Department`, `JobTitle`, `Leave`, `Absent`, `NightShiftTeam`, `TransferLog`, `AuditLog`, `SystemSetting`, `ApplicationUser` (extended IdentityUser). All use `isDeleted` (`bool`) soft-delete with `IgnoreQueryFilters()` patterns |
| `DTOs/` | 16+ DTO files (16 files; `EmployeeFullDTO`, `LeaveFullDto`, `AbsentFullDto`, `TransferLogDto`, `CreateDepartmentDto`, `PagedResult<T>`, `EmployeeLookupDto`, plus others) |
| `Enums/` | `Enums.cs` — all domain enums: `enShiftType` (Morning/Night), `enGender`, `enCertificate`, `enLeaveType`, `enJobStatus`, `enCertificate`, `enAuditType`, `enMorningShifts`, `enNightShiftId` |
| `Hospital.Core.csproj` | net8.0 class library; refs `Microsoft.Extensions.Identity.Stores` (8.0.10) + `Serilog.AspNetCore` (8.0.0) — packages only; no EF |

### 2.2 `Hospital.API/` (Web API — REST backend)
| Path | Contents |
|---|---|
| `Program.cs` | Bootstrap — Identity + JWT auth, Serilog, Swagger, CORS, DbSeeder roles/admin at startup |
| `Controllers/` | `AuthController`, `EmployeesController`, `DepartmentsController`, `JobTitlesController`, `LeavesController`, `AbsentsController`, `TransferLogController` (+PUT 2026-09-21; no DELETE by design), `AuditLogsController`, `ShiftsController`, `NightShiftTeamsController` (PUT hardened 2026-09-21: DTO + Admin + guards). All paged GETs return `PagedResult<T>` (standardized 2026-09-21; fixes Desktop paging) |
| `Data/` | `ApplicationDbContext` (Identity + EF, global query filters, `SaveChangesAsync` audit), `DbSeeder` |
| `Middleware/` | `ExceptionMiddleware` (global handler, Arabic messages) |
| `Migrations/` | 9 migrations + `ApplicationDbContextModelSnapshot` (schema: Absents, AspNetUsers + Identity, Departments, Employees, Leaves, JobTitles, NightShiftTeams, SystemSettings, TransferLogs, AuditLogs; latest `20260402181244_newfeature`) |
| `Services/` | `ShiftService.cs` (delegates rotation math to pure `ShiftCalculator.GetTeamId`), `ShiftCalculator.cs` + `LeaveBalanceCalculator.cs` (pure, unit-tested; used by LeavesController) |
| `Properties/` | `launchSettings.json` (ports: http 5180 / https 7278) |
| `Hospital.APIConfig:` `appsettings.json` + `.Development.json` — **gitignored** (local, contains `Jwt:Key`; do NOT commit) |
| `Hospital.API.csproj` | net8.0; refs Hospital.Core; packages: JwtBearer, Identity, EF (SqlServer/Design/Tools), Serilog.AspNetCore, Swashbuckle |
| `Hospital.API.http` | Manual HTTP test snippet (references `/weatherforecast/` which is **not implemented** — leftover stub) |

### 2.3 `Hospital.Desktop/` (WPF MVVM client)
| Path | Contents |
|---|---|
| `App.xaml` / `App.xaml.cs` | StartupUri => `Views/LoginView.xaml`, RTL, global styles, static `ApiService`/settings |
| `MainWindow.xaml(.cs)` | Shell: RTL sidebar nav + `CurrentView` ContentControl host |
| `Views/` | 22 XAML views, all RTL, fully canonical (2026-09-21): zero inline hex (grep-verified); `Themes/Colors.xaml`+`Typography.xaml`+`Styles.xaml` (+status-only `GovernmentalBrushes`); all buttons `Primary/SecondaryButton`; titles `Header1/Header2`; dark DataGrid headers; home = `DashboardView` |
| `ViewModels/` | 20 VMs (verified 2026-09-21): 19 catalogued + `DashboardViewModel` (guarded parallel aggregates; initial `MainViewModel.CurrentView`; nav callback to sections) — hand-rolled `RelayCommand` (nullable-annotated 2026-09-21), no DI |
| `Services/` | `ApiService` (HTTP + JWT; base URL from `Properties/Settings.settings:ApiBaseUrl` = `https://localhost:7278/api/` since TASK-FE-03, single shared client), `EncryptionHelper` (DPAPI), `ReportGenerator` (FlowDocument A4-Landscape RTL, balanced morning/night tables, 2-col internal split) |
| `Converters/` | 15+ WPF value converters incl. `StatusConverters.cs` (converters 1-15), `BooleanToStatusConverter`, `BoolToVisConverter` |
| `Properties/` | `Settings.settings` + `Settings.Designer.cs` (SavedUsername/SavedPassword/IsRemembered), `.csproj.user`, `Settings.settings` |
| `Hospital.Desktop.csproj` | net8.0-windows, WPF; refs Hospital.Core; packages: CommunityToolkit.Mvvm (8.4.1, *not actually used* — hand-rolled MVVM instead), Microsoft.Extensions.Http, Newtonsoft.Json, Serilog, System.Security.Cryptography.ProtectedData |

---

## 3. Entry Points

| App | File to launch | Notes |
|---|---|---|
| **API** | `dotnet run --project Hospital.API` | Launches on https://localhost:7278 (see launchSettings). `Program.cs` is the bootstrap. |
| **Desktop** | `dotnet run --project Hospital.Desktop` | WPF app; `App.xaml` `StartupUri="Views/LoginView.xaml"`; login first, then `MainWindow` |

**Desktop startup flow:** `App.xaml.cs` → `LoginView.xaml` (login form) → POST `api/Auth/Login` → JWT stored in `ApiService` static → `MainWindow` (sidebar nav) → per-grid `Views` bound to `ViewModels`.

---

## 4. Data Flow (end-to-end, desktop client + API)

```
Hospital.Desktop (WPF)
  LoginView (LoginViewModel) --api/Auth/Login--> [JWT] --> ApiService._token
  EmployeesViewModel.LoadEmployees() --GET api/Employees?IsDeleted=&page=&pageSize=&searchTerm=-->
        EmployeesController (ApplicationDbContext, pagination, filters)
           --EF Core--> SQL Server (HospitalManagementDB)
  <-- PagedResult<EmployeeSimpleDTO> --List/Grid Binding (ObservableCollection + FilteredEmployees)
  Add/Edit -> EmployeeFormView (EmployeeFormViewModel) --POST/PUT api/Employees-->
                validates Department/JobTitle -> SaveChangesAsync -> AuditLog captured via DbContext override

Leaves / Absents / Departments / JobTitles / Users / TransferLog / AuditLogs:
  Same pattern: XxxViewModel -> ApiService.GetAsync/PostAsync/PutAsync/DeleteAsync -> api/Xxx -> Controller -> DbContext -> SQL
```

**Default flow (all collections):**
1. ViewModel constructor → `LoadXxx()` (async) → `_apiService.GetAsync<PagedResult<…>>("Xxx?…&page=&pageSize=")`
2. Controller applies filters (search, deleted flag, date) → paged EF query → DTO projection
3. Response bound into `ObservableCollection` + `ICollectionView` (`FilteredXxx`, client-side filter/search)
4. Create/Edit/Delete commands → POST/PUT/DELETE → reload list

**Audit flow:** `ApplicationDbContext.SaveChangesAsync` override inspects ChangeTracker entries → only 2-phase save (first save returns IDs, then writes `AuditLog` rows with `UserId` from `HttpContext` claim `NameIdentifier`) → AVOIDS auditing `AuditLog` itself.

---

## 5. Key Modules & Responsibilities

| Module / Area | Project | Responsibility | Primary consumers |
|---|---|---|---|
| AuthN/AuthZ | API | Identity + JWT (Login/Register/ChangePassword/AdminResetPassword/Users CRUD), roles Admin/Manager/User, global `RequireAuthenticatedUser`+ per-action `[Authorize(Roles=…)]` | Desktop login (Desk App), all Desktop VMs |
| Employees | API+Desktop | CRUD + pagination + soft-delete/restore, per-dept counts | HR staff |
| Departments | API+Desktop | CRUD + Manager assignment, staff counts (morning/night) | HR staff |
| Leaves (أجازات) | API+Desktop | CRUD, employee+sub-employee, duration vs LeaveBalance deduction, delete restores balance | HR staff |
| Absents (غيابات) | API+Desktop | CRUD + filters (name/date/status) + pagination | HR staff |
| JobTitles | API+Desktop | CRUD (Admin) | HR staff |
| TransferLog (التنقلات) | API+Desktop | CRUD, old/new dept + shift, Employee nav, pagination | HR staff |
| AuditLogs (سجلات التدقيق) | API+Desktop | Read-only, paginated, includes user+type | Admin |
| NightShiftTeams / ShiftSettings | API+Desktop | Night-shift teams, supervisor, modulo-4 team rotation `ShiftService.GetTeamIdByDate` | Shift settings |
| Auth Users | API+Desktop | Users CRUD (Admin), role mgmt, activate/delete | Admin |

---

## 6. Fast Reference: Key Implementation Points

- **Soft-delete everywhere:** domain entities have `isDeleted`; DbContext `HasQueryFilter(e => !e.isDeleted)` + `IgnoreQueryFilters()` for deleted views; controllers pass `IsDeleted` filters; desktop `DeleteCommand` = soft-delete; restore = PUT with `isDeleted=false`.
- **Arabic-first UI:** all Arabic messaging (`MessageBox.Show("…")`), RTL XAML, `FlowDirection="RightToLeft"`. All API controllers also return Arabic error messages.
- **DTO naming is inconsistent (real, keep as-is in docs):** mix of `EmployeeFullDTO`, `EmployeeFullDto`, `LeaveFullDto`, `JobTitleViewDTO` (renamed from `JobTitleVeiwDTO` on 2026-09-20), `CreateEmployeeDTO` vs `CreateAbsentDto` - do not "fix" in docs; mind both.
- **Tests EXIST (2026-09-21):** `Hospital.Tests/` (xUnit + Moq + FluentAssertions, net8.0) — 35/35 green via `dotnet test HospitalProject.slnx`. CI `build.yml` + `README` + `.editorconfig` + `LICENSE` (MIT) exist since 2026-09-20.
- Build 2026-09-21: `dotnet build HospitalProject.slnx` → 0 errors, 110 warnings (nullable CS8600/8603/8604/8625/8629 + CS0168; CS8981 lowercase `en*` kept as contract).
- `Hospital.sql` is the **DB seed/backup** (full scripted schema). `Migrations/` are the authoritative EF schema.
- Serilog `Logs/` is generated at runtime (`Logs/log-.txt` rolling); `.gitignore` has `logs/` (lowercase) — API writes to `Logs/`; watch case.

---

## 7. Maintainers / Consumers

| Area | Maintainer | Consumers |
|---|---|---|
| Hospital.Core | (repo owner — local) | API, Desktop, future mobile/service |
| Hospital.API | (repo owner — local) | Desktop client |
| Hospital.Desktop | (repo owner — local) | Hospital admin users |

> Maintainer identities are **not documented** in the repo (no CONTRIBUTING). This table records the owner currently working in the local clone.

---

## 8. Navigation Cheat-Sheet (Top Files to Read First)

1. `Hospital.API/Program.cs` — auth + services wiring (JWT, Serilog, Swagger, CORS, seeder)
2. `Hospital.API/Data/ApplicationDbContext.cs` — schema mapping, soft-delete filters, audit SaveChanges
3. `Hospital.API/Controllers/EmployeesController.cs` — canonical CRUD + paginated pattern used by every other controller
4. `Hospital.Desktop/Services/ApiService.cs` — the single HTTP + JWT client shared by all VMs.
5. `Hospital.Desktop/ViewModels/EmployeesViewModel.cs` — canonical MVVM list VM (paging, search, filters, commands)
6. `Hospital.Core/Models/Employee.cs` + `Hospital.Core/Enums/Enums.cs` — domain kernel
