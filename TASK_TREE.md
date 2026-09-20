# TASK_TREE.md ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Production-Readiness Execution Tree
*Project: Hospital Management System (HospitalProject)*
*Status: Phase 3 in progress — Boundary A COMPLETE, Boundary B audit+rename done*
*Authoritative Source: verified baseline audit (10 controllers, 19 VMs, 23 views, 9 migrations, ~258 nullable warnings)*

---

## ÃƒÂ°Ã…Â¸Ã¢â‚¬â€Ã‚ÂºÃƒÂ¯Ã‚Â¸Ã‚Â Isolation Strategy
To ensure stability, this tree is divided into **Isolation Boundaries**. Tasks within a boundary may touch shared files, but tasks across boundaries must be executed sequentially. No two tasks from different boundaries shall be executed concurrently.

---

## ÃƒÂ°Ã…Â¸Ã…â€™Ã‚Â³ Execution Tree

### Boundary A: Infrastructure & Governance (Zero Code Risk)
*Isolated from runtime behavior. Pure file-level setup.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|---|
| `TASK-INF-01` | Root / README | None | Create `README.md` including project purpose, setup, and entry points (net8.0, ports 7278/5180) | File exists, content traces to `PROJECT_MAP.md` | `[DONE]` |
| `TASK-INF-02` | Root / Config | None | Create `.editorconfig` defining 4-space indent, UTF-8, and C# coding style (matching existing patterns) | `dotnet build` (verify no new warnings) | `[DONE]` |
| `TASK-INF-03` | Root / Legal | None | Create `LICENSE` (User Decision Required) | File exists, license type confirmed by user | `[DONE]` |
| `TASK-INF-04` | `.github/` | `TASK-INF-01` | Create `.github/workflows/build.yml` for automatic `dotnet build HospitalProject.slnx` (User Decision Required) | GitHub Action trigger (simulated via local check) | `[DONE]` |

### Boundary B: Core Domain & Schema (Low Risk)
*Changes to shared contracts. Impacts API and Desktop.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|
| `TASK-CORE-01` | `Hospital.Core/DTOs` | None | Fix naming inconsistency: `JobTitleVeiwDTO` typo (Wait: documented as part of contract in ARCHITECTURE.md. Decision required if this is a "production gap" or "contract constant") | Done 2026-09-20: renamed in 5 files/13 usages; JSON wire format unchanged | `[DONE]` |
| `TASK-CORE-02` | `Hospital.Core/Models` | None | Audit all `isDeleted` properties to ensure consistent naming (currently `isDeleted` lowercase) | `Done 2026-09-20 (audit only): 4x isDeleted + ApplicationUser.IsDeleted outlier + 5 entities with no flag (AuditLog/JobTitle/NightShiftTeam/SystemSetting/TransferLog, likely intentional); no rename (would force EF migration)` | `[DONE]` |

### Boundary C: API Layer (Medium Risk)
*Changes to REST endpoints and Data access.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|
| `TASK-API-01` | `Hospital.API/Controllers/EmployeesController.cs` | None | Enum-range + NightShiftId-FK validation on POST/PUT (duplicate check rejected: no unique index in ApplicationDbContext) | `dotnet build` green + 400-path code review (live Swagger check needs running API+DB) | `[DONE]` |
| `TASK-API-02` | `Hospital.API/Controllers/AuthController.cs` + `Hospital.Core/DTOs/RegisterDTO.cs` | None | Register DTO [Required]/MinLength; EmployeeId FK-guard (FK_AspNetUsers_Employees_EmployeeId) in Register/UpdateUser; role-exists + role-result checks in UpdateUser. Complexity NOT a gap (Identity defaults); FullName-null crash unreachable (NOT NULL col + seed) -> skipped | `dotnet build` green + code review | `[DONE]` |
| `TASK-API-03` | `Hospital.API/Data/ApplicationDbContext.cs` | None | Index review: ALL FKs already indexed by EF convention (snapshot-verified: DepartmentId/JobTitleId/NightShiftId/ManagerId/all EmployeeId/Old+NewDepartmentId/SupervisorId). Only candidate IX_AuditLogs_Date deferred (unmeasured, needs migration+DB update). No migration = no schema churn | Snapshot grep + `dotnet build` green | `[DONE]` |

### Boundary D: Desktop Layer (Medium Risk)
*Changes to WPF UI and MVVM logic.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|
| `TASK-FE-01` | `Hospital.Desktop/ViewModels` | None | Replace hand-rolled `RelayCommand` with `CommunityToolkit.Mvvm` if approved — DECLINED by standing rule (AGENTS.md §4 DON'T: hand-rolled MVVM is the standard; toolkit stays an unused reference). No code change | Contract check | `[DONE]` |
| `TASK-FE-02` | `Hospital.Desktop/Views` | None | Audit all views for missing RTL `FlowDirection="RightToLeft"` consistency | Grep audit 2026-09-20: 21/21 views RightToLeft-only; no change | `[DONE]` |
| `TASK-FE-03` | `Hospital.Desktop/Services/ApiService.cs` + `Properties/Settings.settings` + `Settings.Designer.cs` | None | Move single-source base URL to Application-scope `ApiBaseUrl` setting (default = current URL, behavior-identical) | `dotnet build` green | `[DONE]` |

### Boundary E: Quality Assurance & Stability (High Risk)
*Cross-cutting changes; requires full regression test.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|
| `TASK-QA-01` | All Projects | None | Cleanup 258 build warnings via bounded passes (true baseline re-verified 2026-09-20 after removing self-inflicted analyzer-severity override from .editorconfig; bulk = CS8618). Pass order: API (6 sites) -> Core (134) -> Desktop (140). Pass A DONE 2026-09-20: CS8618 85->0 via `= null!` initializers + nullable event decls (`?`, all invocations verified `?.Invoke`). Emitted 258->110, unique 152->63, 0 errors, Migrations untouched. Counting note: emitted totals double-count WPF (normal+wpftmp builds); unique is the true metric. Next: Pass B = CS0168(3, safe removal) + dereference group CS8604/8625/8603/8601/8600/8629/8602/CS8767/CS8612 (~50 sites, per-file judgment). CS8981 (4, lowercase `en*` enum names) DECLINED — renaming = API+Desktop contract churn | `dotnet build` warning count < 10 | `[IN_PROGRESS]` |
| `TASK-QA-02` | Root / Tests | None | Create xUnit/NUnit test project; implement basic unit tests for `ShiftService` and `AuthController` | `dotnet test` success | `[PENDING]` |
| `TASK-QA-03` | All Projects | `TASK-QA-01` | Full clean build: `dotnet clean && dotnet build` | `dotnet build` success (0 errors) | `[PENDING]` |

---

## ÃƒÂ°Ã…Â¸Ã…Â¡Ã¢â€šÂ¬ Execution Loop State
**Current Task:** `None`
**Active Boundary:** `Boundary D — COMPLETE; Boundary E — IN PROGRESS` (FE-03 done)
**Next Actionable:** `TASK-QA-01` (IN PROGRESS: warning inventory first, then bounded passes)
