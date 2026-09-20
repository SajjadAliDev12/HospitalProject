# rules.md — Coding Rules & Conventions (HospitalProject)

> Companion to `AGENTS.md` (workflow contract), `ARCHITECTURE.md` (blueprint), `PROJECT_MAP.md` (navigation), `TECH_STACK.md` (versions). This file is the **mechanical rulebook**: style, naming, DI, validation, error handling, and the allowed/forbidden libraries. Every rule below is derived from how the actual code is written today — with absent pieces explicitly marked **Not Implemented**.
> Repo root: `C:\Users\scorp\source\repos\HospitalProject` · Solution: `HospitalProject.slnx` (XML `.slnx`, no classic `.sln`).

---

## 1. Style & Formatting

### 1.1 C# basics (as the code actually looks)
- **Language/TFM:** C# 12, `net8.0` (API/Core), `net8.0-windows` (Desktop). **All 3 projects have `Nullable` + `ImplicitUsings` enabled.**
- **Allman braces; 4-space indent** (matches every `.cs` file in the repo).
- **`System.*` usings top; then project usings; blank line between groups** (see any controller/VM).
- **Files:** BOM-less UTF-8 with Arabic RTL strings inline (RTL comes from XAML/VM layout, not from file bytes).
- No `.editorconfig`, no `.editorconfig`-driven analyzer config, no StyleCop/Analyzer packages — **code style is convention-only. Not Implemented: analyzer enforcement.**

### 1.2 Naming (real, keep as-is — DO NOT "fix")
| Thing | Reality | Examples |
|---|---|---|
| Namespaces | `Hospital.Core`, `Hospital.API`, `Hospital.Desktop` + subfolders | `Hospital.Core.DTOs` |
| Entities | PascalCase class names; **mixed-casing tolerated** | `Employee`, `Department`, `JobTitle` |
| DTOs | **inconsistent suffix** (`DTO` vs `Dto`) - keep | `EmployeeFullDTO`, `EmployeeFullDto`, `LeaveFullDto`, `JobTitleViewDTO` |
| Enums | **Arabic-scoped `en`-prefixed** then Pascal member | `enGender { Male, Female }`, `enShiftType { Morning, Night }`, `enJobStatus { Continuous, OnVacation, DisContinue, Retired, Departed }`, `enLeaveType { Hourly, SickLeave, Motherhood, Normal, AfterBirth, Yearly, WithOutSalary }`, `enCertificate { HighSchool, institute, Collage, Master, PHD, Prof }`, `enRole { Admin, Manager, User }`, `enAuditType { Add, Edit, Delete, Transfer, Leave, Absent }`, `enMorningShifts { SaturdayGroup, ThursdayGroup }` |
| Fields / props | PascalCase; **soft-delete spelled `isDeleted`** (lowercase `isDeleted`!). | `isDeleted`, `isActive`, `ManagerOrderNumber` |
| Local-ish vars | camelCase | `userRoles`, `authClaims` |
| Arabic strings | Arabic-first, full Arabic sentences in messages/labels/reports; RTL on Desktop | `message = "المستخدم غير موجود"` |

> **Mind legacy DTO naming** (mixed `DTO`/`Dto` suffixes): they are **part of the API + Desktop contract**. Never rename them as a drive-by; mirror existing names.

### 1.3 Soft-delete (universal convention)
- **Every domain entity** has `isDeleted` (`bool`). EF `ApplicationDbContext` has `HasQueryFilter(e => !e.isDeleted)` on soft-deletable entities; Desktop + controllers use `IgnoreQueryFilters()` when they need to read/restore deleted rows.
- **Rules:** never hard-delete in a controller; soft-delete via `isDeleted=true`; "deleted" views + restore use `IgnoreQueryFilters()`; audit entries capture the change via the `SaveChangesAsync` override.

---

## 2. Solution Command Rules (canonical commands)

| Command | When | Notes |
|---|---|---|
| `dotnet build HospitalProject.slnx` | after every change | **Must stay green**; warnings expected (~258 nullable warnings). Build the **whole solution**, not one project. |
| `dotnet run --project Hospital.API` | run API | Launches `https://localhost:7278` (https) / `http://localhost:5180` (http) — see `launchSettings.json`. |
| `dotnet run --project Hospital.Desktop` | run Desktop | WPF `net8.0-windows`; `StartupUri` => `Views/LoginView.xaml`. |
| `dotnet ef migrations add <Name>` (run from `Hospital.API/`) | schema change | Creates a new migration in `Hospital.API/Migrations/`. |
| `dotnet ef database update` (from `Hospital.API/`) | apply changes | Runs migrations against `HospitalManagementDB`. |
| `dotnet ef migrations list` | inspect | Shows applied/pending migrations. |

> **Ports:** API = `https://localhost:7278` (api base URL hardcoded in Desktop `ApiService` = `https://localhost:7278/api/`). **Do not invent ports.**

---

## 3. Dependency Injection & Lifecycle (API)

- Identity + JWT + Serilog + Swagger wired in `Hospital.API/Program.cs`.
- **Controllers are the DI boundary** — inject `ApplicationDbContext` via constructor; **no repository/UoW layer exists** (controllers speak directly to the DbContext — this is the codebase reality; do NOT add one without a conversation).
- **Services (API):** registered via `AddScoped`/`AddSingleton` in `Program.cs` when present (e.g., `ShiftService`, `ApplicationDbContext`).
- **Desktop:** hand-rolled MVVM; no DI container — VMs `new ApiService()` directly; `ApiService` is a singleton static-ish HTTP client shared by all VMs.
- **Lifecycle expectations:** `ApplicationDbContext` is `AddDbContext` → **scoped**; controllers get a fresh instance per request; `IHttpContextAccessor` used by `ApplicationDbContext` audit override.

---

## 4. Error Handling (canonical pattern)

1. **API:** every POST/PUT/DELETE wraps logic in `try { … } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }` — controllers return Arabic `{message}` on the expected failure paths.
2. **Global middleware:** `Hospital.API/Middleware/ExceptionMiddleware.cs` (`UseMiddleware` in `Program.cs`) catches unhandled exceptions, logs via Serilog, returns Arabic `{message}` + HTTP 500.
3. **Validation:** `[ApiController]` automatic 400 with Arabic model-state messages (DataAnnotations on DTOs); plus manual `if (… == null) return BadRequest/NotFound` checks in controllers.
4. **Desktop:** VMs catch `Exception` from `ApiService` and `MessageBox.Show(ex.Message)` (Arabic); commands gate via `RelayCommand` `CanExecute`.
5. **Never return raw English stack traces to the client** — Arabic `message` only.

---

## 5. Allowed / Forbidden libraries

| Library | Where | Allowed? | Notes |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` + Design/Tools | API | ✅ | EF Core 8.0.10 |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | API | ✅ | ASP.NET Identity + IdentityRole |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | API | ✅ | JWT auth 8.0.10 |
| `Microsoft.Extensions.Identity.Stores` | Core | ✅ | Identity domain |
| `Serilog.AspNetCore` | API/Core/Desktop | ✅ (API only at runtime) | API logs via Serilog (Console + `Logs/` file); Desktop references it but **Serilog not initialized in Desktop** (todo). |
| `Swashbuckle.AspNetCore` (6.6.2) | API | ✅ | Swagger + JWT bearer |
| `Newtonsoft.Json` (13.0.4) | Desktop | ✅ | JSON on Desktop |
| `CommunityToolkit.Mvvm` (8.4.1) | Desktop | ⚠️ referenced, **NOT used** | Desktop uses hand-rolled `BaseViewModel`/`RelayCommand` MVVM. Do NOT assume toolkit availability; keep hand-rolled MVVM. |
| `System.Security.Cryptography.ProtectedData` | Desktop | ✅ | DPAPI for remember-me password |
| `Microsoft.Extensions.Http` (8.0.0) | Desktop | ✅ | `AddHttpClient`/`ApiClient` |
| Any repository/UoW/bus-layer package | — | ❌ | Not part of the architecture; requires a conversation first. |
| Any test/CI/coverage package | — | ❌ Not-implemented | No test project, no CI, no `.editorconfig` today. Flag before adding. |

---

## 6. Do / Don't quick list

**Do:**
- Add Arabic message strings; new UI stays RTL / Arabic-first.
- Follow the thin-controller pattern; inject `ApplicationDbContext`.
- Honor soft-delete; use `IgnoreQueryFilters()` for deleted/restore.
- Keep `isDeleted`/`isActive` in DTOs as real columns are named.
- Match legacy DTO naming as it is (mixed `DTO`/`Dto` suffixes).
- Run `dotnet build HospitalProject.slnx` + check `git status` before finishing.

**Don't:**
- Don't "fix" DTO/enum/entity typos as a drive-by.
- Don't add a repository/business layer without explicit consent.
- Don't assume CommunityToolkit.Mvvm is usable (it's unused despite the reference).
- Don't commit `appsettings*.json` (`Jwt:Key`, connection string) or `Logs/` — all gitignored/local.
- Don't invent endpoints, ports, or fixtures that aren't in the code.

---

## 7. Definition of Done
- `dotnet build HospitalProject.slnx` succeeds (warnings ~258 acceptable).
- No secrets / build artifacts in `git status`.
- Arabic strings in every new user-facing message.
- Docs updated with facts traceable to real code only.
