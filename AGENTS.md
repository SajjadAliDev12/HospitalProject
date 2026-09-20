# AGENTS.md — Working Agreement for Agents on HospitalProject

*Companion to `PROJECT_MAP.md` (navigation) and `ARCHITECTURE.md` (blueprint). Read all three before touching code. Everything below is a **workflow contract**, not a summary of the codebase — for what the code actually is, see the two companion docs.*

---

## 1. Agent Identity

You are an **agent working on the HospitalProject codebase** (`C:\Users\scorp\source\repos\HospitalProject`, a 3-project .NET 8 hospital HR/shift-management system: WPF Desktop client + ASP.NET Core Web API + shared Core library). Your job is to make correct, minimal, **source-traceable** changes and to keep the documentation honest.

The repository has **no maintainer metadata and no tests**. `README.md`, `.editorconfig`, `LICENSE` (MIT), and CI (`.github/workflows/build.yml`) were added 2026-09-20 (TASK-INF-01..04) — declare them explicitly in summaries. Treat remaining gaps as gaps — **never** as things that already exist.

---

## 2. Ground Rules (Non-Negotiable)

1. **Never invent.** Every implementation claim you make about this repo must trace back to a real file. If a feature/layer doesn't exist, say "Not implemented" — do not describe a fantasy version.
2. **Verify with real commands before you change something.** The canonical checks:
   - `dotnet build HospitalProject.slnx` — must stay green (warnings are expected; currently ~258 nullable warnings).
   - `dotnet ef migrations list` / `dotnet ef database update` — run from `Hospital.API/`.
3. **Read the code first.** When you change behavior, base it on what controllers/VMs/context *actually do* today, not on what you wish they did.
4. **Know the conventions table (below)** - the repo is half-Arabic and keeps legacy quirks (mixed `DTO`/`Dto` suffixes) as-is. Match existing style; **do not "fix" naming as a drive-by**. (Historical: `JobTitleVeiwDTO` renamed to `JobTitleViewDTO` on 2026-09-20, TASK-CORE-01.)
5. **Secrets stay out.** `appsettings.json`, `appsettings.Development.json`, and anything under `Logs/` are gitignored. A `Jwt:Key`, connection string, or password must **never** be committed. When you see one, keep it local-only.
6. **`dotnet build` is your friend, and so is `git status`.** Before announcing success, build and check `git status` to confirm no unintended files (e.g., `Logs/`, `.sql` dumps) sneak in.
7. **One domain per project.** Hospital.Core = models/DTOs/enums only. Don't add HTTP or EF to Core. Don't add repositories "just because" — the API uses `ApplicationDbContext` directly.
8. **Ask the human for anything unexpected or ambiguous (gitignored secrets, ports, structure questions).** Don't extrapolate.

---

## 3. Repository Conventions (Reality Check)

| Area | What the code actually does |
|---|---|
| **Soft-delete** | Every domain entity has `isDeleted`; `ApplicationDbContext` has `HasQueryFilter(e => !e.isDeleted)` on the soft-deletable entities, and `IgnoreQueryFilters()` is used on purpose in controllers. Always honor soft delete; do not hard-delete. |
| **Symmetry between layers** | Desktop `Services/ApiService.cs` ↔ API controllers ↔ EF `ApplicationDbContext`. VMs `new ApiService()` directly; no DI container on Desktop. |
| **Authentication** | JWT Bearer; all endpoints require auth globally (global `AuthorizeFilter`, no `AllowAnonymous` default). Roles: `Admin` (seeded), plus `Manager`, `User`. |
| **Arabic-first UI** | Messages, labels, reports, and error strings are Arabic. New Arabic string constants should be added to the source with correct handling (and RTL layout for Desktop). |
| **Dateonly** | Entities use `DateOnly` for dates (BirthDate, HireDate, ShiftReferenceDate etc.) — EF maps to SQL `date`; Desktop uses a `DateOnly` converter in JSON handling. |
| **DTO naming quirks** | DTOs have inconsistent casing (mixed `DTO` vs `Dto` suffixes, `PagedResult<T>` etc.). Keep those as-is; mirror the existing names when adding new ones. (Historical: `JobTitleVeiwDTO` fixed to `JobTitleViewDTO` on 2026-09-20.) |
| **Shift rotation** | `ShiftService` computes night-shift team by date: `(daysSinceReference % 4)` → team 1..4, with `NightShiftTeam` seeded rows 1-4 comma `SystemSettings.ShiftReferenceDate`. The shift teams rotate on the reference date. Desktop also has a client-side `ShiftService`. |
| **Auditing** | `ApplicationDbContext.SaveChangesAsync` writes `AuditLog` rows for add/edit/delete/transfer/leave/absent/audit-type changes. |

---

## 4. Do / Don't (Behavior Contract)

**DO:**
- Add Arabic UI strings & keep RTL for any new Desktop screen.
- Follow existing controller pattern: inject `ApplicationDbContext` (API) / use `ApiService` (Desktop); manual `try/catch` returning Arabic `{message}` on POST/PUT/DELETE.
- Use `IgnoreQueryFilters()` deliberately when you need to read soft-deleted rows (e.g., RESTORE logic).
- Respect the role-guarding: new sensitive endpoints get `[Authorize(Roles = "Admin")]` (or `Admin,Manager`).
- Keep `Nullable` semantics as the repo does (nullable context enabled on all 3 projects; API uses non-nullable principal entities).
- Check in documentation updates that match the actual tree (see `PROJECT_MAP.md` for the current tree update loop).

**DON'T:**
- Do NOT introduce a repository/UoW/business layer without a dedicated conversation first — controllers are the data access.
- Do NOT use `CommunityToolkit.Mvvm` in Desktop — even though it's referenced in csproj, the codebase uses hand-rolled MVVM (`BaseViewModel`/`RelayCommand`). Match that.
- Do NOT commit `appsettings*.json`, `Logs/`, or any local secrets.
- Do NOT "fix" legacy DTO naming quirks (mixed `DTO`/`Dto` suffixes) - they are part of the API contract. (Executed exception 2026-09-20: `JobTitleVeiwDTO` -> `JobTitleViewDTO`, TASK-CORE-01, wire format unchanged.)
- Do NOT hardcode the base URL again — it's already `https://localhost:7278/api/` in `ApiService`; change centrally if needed (currently single source).
- Do NOT assume Identity roles named anything other than `Admin`/`Manager`/`User` (seeded by `DbSeeder`).
- Do NOT silently rename migrations or alter the EF `ModelSnapshot` — migrations are the schema truth.

---

## 5. Per-Module Workflow (Pattern for New Changes)

The canonical "add a feature" flow, matching how the existing modules are built:

### 5.1 Backend (Hospital.API)
1. **Model + Migration:** add entity to `Hospital.Core/Models/`; add `DbSet<>` in `ApplicationDbContext`; run `dotnet ef migrations add <Name>` from `Hospital.API`; check the migration's `Up`/`Down` and the snapshot update.
2. **Controller:** create `Hospital.API/Controllers/XxxController.cs`; inject `ApplicationDbContext`; every action `[Authorize]` with the correct `Roles`; mirror existing `GET (paged/filtered)` + `POST` + `PUT {id}` + `DELETE {id}` shape. Return Arabic `{message}` on errors; `Ok` on success.
3. **DTO:** put request/response DTOs in `Hospital.Core/DTOs/` (match existing naming; tolerate typos).
4. **Services:** if the feature has domain logic (e.g., shift calc is deliberately in `ShiftService`), co-locate it under `Hospital.API/Services/`, register via DI in `Program.cs` (or `AddScoped`).
5. **Verify:** `dotnet build HospitalProject.slnx` + hit endpoint in Swagger with a Bearer token.

### 5.2 Frontend (Hospital.Desktop)
1. **View:** add `Views/XxxView.xaml` (+ `.xaml.cs`); RTL, Arabic labels, bind via existing converters (`Converters/StatusConverters.cs` & friends) & `DateOnlyJsonConverter` as needed.
2. **ViewModel:** add `ViewModels/XxxViewModel.cs` derived from `BaseViewModel`; implement `LoadXxx()` async calling `ApiService.GetAsync<PagedResult<Xxx>>(...)`; commands via `RelayCommand`; paging via `CurrentPage`/`TotalPages` if list.
3. **Wire nav:** register VM→View `DataTemplate` in `App.xaml`; add sidebar entry in `MainViewModel`/`MainWindow`.
4. **Verify:** run Desktop + API together; be mindful the desktop talks to hardcoded base URL.

### 5.3 Cross-cutting
- Audit: new write operations are captured automatically by the `SaveChanges` override — no extra work unless new entity types need mapping in the audit switch.
- Serilog: API logs via Serilog (console + `Serilog` files). Desktop: **Serilog not configured** in Desktop, though package is referenced.

---

## 6. Context-Loading Protocol (minimal file list)

Read in this order before making a change:

| Purpose | Files |
|---|---|
| Auth & JWT wiring | `Hospital.API/Program.cs`, `Hospital.API/Controllers/AuthController.cs`, `Hospital.API/Data/DbSeeder.cs` |
| EF & audit & soft-delete | `Hospital.API/Data/ApplicationDbContext.cs`, `Hospital.API/Data/ApplicationDbContextModelSnapshot.cs` (schema reference), `Hospital.API/Middleware/ExceptionMiddleware.cs` |
| Domain/DTO/enums | `Hospital.Core/Models/*.cs`, `Hospital.Core/DTOs/*.cs`, `Hospital.Core/Enums/Enums.cs` |
| Shift rotation | `Hospital.API/Services/ShiftService.cs` (API side), desktop-side `Hospital.Desktop/...ShiftService` |
| Desktop HTTP + MVVM base | `Hospital.Desktop/Services/ApiService.cs`, `Hospital.Desktop/ViewModels/BaseViewModel.cs` |
| Endpoint patterns | pick `EmployeesController.cs` as canonical; scan others for role variants |

---

## 7. Definition of Done

- Solution builds: `dotnet build HospitalProject.slnx` → **success** (existing warnings acceptable).
- No secrets or build-artifacts staged (check `git status`).
- New string literals Arabic; Desktop UI stays RTL.
- Soft-delete honored; no hard deletes unless explicitly requested.
- Docs updated in `PROJECT_MAP.md`/`ARCHITECTURE.md` only with facts that now exist; anything removed place-holdered as "Removed/Not implemented".
- If you created `tests`, `CI`, `.editorconfig` → **you must say so explicitly** in your summary (no test project exists today; README/`.editorconfig`/LICENSE/CI were added 2026-09-20 and are already declared).

---

## 8. Communication Style

- Respond to the user in their language (this session's UI is Arabic-first; use Arabic where the user does).
- Use concrete file paths (e.g., `Hospital.API/Controllers/EmployeesController.cs`), not vague references.
- Keep answers short and precise; the docs are the deep reference.
- When in doubt, ask before writing code — especially around structure/secrets/ports.
