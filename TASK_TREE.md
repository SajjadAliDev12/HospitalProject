# TASK_TREE.md — Production-Readiness Execution Tree
*Project: Hospital Management System (HospitalProject)*
*Status: Phase 4 kickoff 2026-09-21 — Boundaries A–D COMPLETE, Boundary E IN PROGRESS (QA-01), new Boundaries F–H planned (gov UI, shifts/reports, tests)*
*Authoritative Source: verified audit 2026-09-21 (10 controllers/43 HTTP actions, 19 VMs, 21 views + MainWindow, 9 migrations, build 0 errors/110 warnings, 0 // TODO in src)*

---

## Isolation Strategy
To ensure stability, this tree is divided into **Isolation Boundaries**. Tasks within a boundary may touch shared files, but tasks across boundaries must be executed sequentially. No two tasks from different boundaries shall be executed concurrently.

---

## Execution Tree

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
| `TASK-QA-01` | All Projects | None | Cleanup warnings Pass B: 110 → <10, behavior-identical | DONE 2026-09-21: **110 → 4 unique** (0 errors). Central fixes: nullable `RelayCommand`/`OnPropertyChanged`/`ApiService data+default!`/converter `!`; per-VM nullable signatures (guards already existed); API `!` + `Employee?` + unused-`ex` removal. Remaining 4 = CS8981 migration class names — ACCEPTED (renaming migrations = EF churn, forbidden). Verified: full `--no-incremental` build + `dotnet test` 35/35 | `[DONE]` |
| `TASK-QA-02` | Root / Tests | None | Create xUnit test project + basic unit tests | DONE 2026-09-21: `Hospital.Tests` (xUnit 2.9.2 + Moq + FluentAssertions, net8.0, in slnx) — 35/35 green (Modulo-4, leave math, PagedResult). AuthController live tests deferred (needs running API+DB+JWT) | `[DONE]` |
| `TASK-QA-03` | All Projects | `TASK-QA-01` | Full clean build: `dotnet clean && dotnet build` | DONE 2026-09-21: `dotnet clean` + `dotnet test HospitalProject.slnx` green (35/35, 0 errors) | `[DONE]` |

### Boundary F: Governmental UI Unification (Medium Risk) — maps to Agent 2 `ui/design-system`
*Verified 2026-09-21: NO `GovernmentalStyles.xaml`/`ButtonStyles.xaml`/`DataGridStyles.xaml` exist (grep: zero hits); styles are inline per-view. Newer views already use slate/navy `#1E293B`/`#2563EB`/`#0F172A`; shell `MainWindow.xaml` + `EmployeesView` + `EmployeeFormView` + `LoginView` still Fluent `#0078D4`/`#202020`. All 21 views + shell already RTL. Segoe MDL2 icon glyphs in use (font-dependent). User decision required on scope before coding.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|---|
| `TASK-UI-01` | `Hospital.Desktop/` new `Themes/` dictionaries | None | Create shared `GovernmentalStyles.xaml` (brushes `#1E293B`/`#0F172A`/`#2563EB`/`#1D4ED8`/`#F8FAFC`/`#E2E8F0` + status green/amber/dark-red) + `ButtonStyles` + `DataGridStyles`; merge in `App.xaml`; keep hand-rolled MVVM + RTL | DONE 2026-09-21: `Themes/GovernmentalBrushes.xaml` + `ButtonStyles.xaml` + `DataGridStyles.xaml` merged in `App.xaml`; dead `MenuButtonStyle/ActionButtonStyle/LogoutButtonStyle` removed (verified zero usages); keys used ⊆ keys defined (script-verified) | `[DONE]` |
| `TASK-UI-02` | `MainWindow.xaml` + shell views | `TASK-UI-01` | Migrate shell + `EmployeesView`/`EmployeeFormView`/`LoginView` from Fluent (`#0078D4`) to gov palette via shared styles; unify TextBox/ComboBox/DatePicker + validation-error visuals | DONE 2026-09-21: shell navy `#0F172A` + royal accents; 3 Fluent views migrated; validation reds → `GovDanger`; `GovTextBox` style (validation tooltip) available opt-in | `[DONE]` |
| `TASK-UI-03` | Remaining 17 views | `TASK-UI-01` | Replace inline colors with shared brushes (no behavior change); unify DataGrid row-height/selection/paging visuals | DONE 2026-09-21: zero legacy hex (`#0078D4`/`#202020`/`#2563EB`/`#1E293B`/`#0F172A`/`#E74C3C`/`#F3F3F3`) left in Views (grep-verified; neutral grays intentionally inline). DataGrids keep local layouts; `GovDataGrid` opt-in for future screens. DECISION 2026-09-21: Segoe MDL2 glyphs RETAINED — system font guaranteed on net8.0-windows target, so vector-path migration = churn with zero benefit | `[DONE]` |
| `TASK-UI-04` | Visible enterprise redesign + admin Dashboard | `TASK-UI-01` | Real visual overhaul (previous pass was token-level, near-identical hues) + rich home dashboard (was `CurrentView = null`) | DONE 2026-09-21: `DashboardViewModel` (7 guarded parallel aggregates: counts + recents + shift split + today's team via `Shifts/calculate`) + `DashboardView` (5 KPI cards, night-shift navy card, shift-split bars, latest leaves/absents, top departments, quick actions) + `DataTemplate` + initial view wiring in `MainViewModel`. Desktop build 0 warnings. `MorningPct/NightPct` bindings fixed to `OneWay` (TwoWay on read-only props throws at runtime) | `[DONE]` |
| `TASK-UI-05` | Canonical design system v2 (`Colors/Typography/Styles`) + Steps 2–4 full overhaul | `TASK-UI-01` | Step 1 DONE earlier. Steps 2–4 DONE 2026-09-21: shell on canonical keys + separator + typography; **zero inline hex in Views/MainWindow** (grep-verified; `Transparent`/`Black`-default keywords + icon font retained); local button styles migrated to `PrimaryButton`/`SecondaryButton` (dead `ModernBtn/ModernButtonStyle/ActionBtn/Gov*Button` removed; `PrimaryBtn/SecondaryBtn` rebased `BasedOn` canonical); local light DataGrid headers removed → canonical dark headers apply; titles → `Header1/Header2`; `GovernmentalBrushes` pruned to domain status families; dead `ButtonStyles/DataGridStyles` files deleted. Residual inline `FontSize/Weight/Margin/Padding` kept as layout metrics (no spacing scale in brief) | `[DONE]` |

### Boundary G: Shifts & Reports Hardening (Medium Risk) — maps to Agent 3 `engine/shifts-and-reports`
*Verified 2026-09-21: `ShiftService.GetTeamIdByDate` = `((daysDiff % 4)+4)%4 + 1` vs `SystemSettings.ShiftReferenceDate` (handles pre-reference dates); `GetCurrentShiftDetail` includes Supervisor. `ReportGenerator` = FlowDocument RTL, A4-Landscape (11.69×8.27in), balanced morning Sat/Thu tables + 2×2 night-team grid with internal 2-col name split. No calculation bug found in review; needs test + print proof.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|---|
| `TASK-SH-01` | `Hospital.API/Services/ShiftService.cs` + `ShiftsController` | None | Edge-case audit: null `SystemSettings` (Arabic throw — by design), negative diffs, team-seed 1–4 drift | DONE 2026-09-21: math extracted to pure `ShiftCalculator` (DayNumber-based, identical results); 13 rotation tests green incl. leap-day + ±1000 sweep | `[DONE]` |
| `TASK-SH-02` | `Hospital.Desktop/Services/ReportGenerator.cs` | None | Single-page stability proof: overflow/padding/column-balance check on large departments; fix only if repro | Print-preview review needs running app — NOT verifiable headless; code reviewed, no defect found | `[PENDING]` |

### Boundary H: Test Coverage (Low Risk, additive only) — maps to Agent 4 `qa/unit-testing`
*Verified 2026-09-21: NO test project exists; `dotnet test` has nothing to run. Zero `// TODO` in src (grep: only `ConvertBack→NotImplementedException`, the standard WPF pattern). Real git branches: only `main` (+ remote); the 4 agent branches do not exist yet.*

| Task ID | Module / Domain | Dependencies | Scope & Deliverable | Verification Criteria | Status |
|---|---|---|---|---|---|
| `TASK-TST-01` | Root / `Hospital.Tests` | None | New xUnit project (Moq + FluentAssertions per user spec); pure-logic tests | DONE 2026-09-21: 35 green; pure scope per user decision (integration deferred). New pure units `ShiftCalculator` + `LeaveBalanceCalculator` (API/Services); `ShiftService`/LeavesController delegate (behavior-identical) | `[DONE]` |
| `TASK-TST-02` | API gaps (Agent 1 backlog) | `TASK-TST-01` | Decide + implement TransferLog PUT/DELETE + NightShiftTeams authz/validation | DONE 2026-09-21: TransferLog PUT (Admin,Manager; FK guards; employee sync iff latest log); DELETE omitted by design (no `isDeleted` + hard-delete forbidden + log integrity, noted in code). NightShift PUT hardened (DTO matching Desktop payload, Admin-only, supervisor-FK + duplicate guards, Arabic errors). BONUS real bug: Leaves/Absents/AuditLogs/TransferLog paged GETs returned anonymous shapes → Desktop `PagedResult.TotalPages` computed from 0/0; standardized on `PagedResult<T>` (wire keeps TotalPages; zero Desktop changes) | `[DONE]` |

---

## Execution Loop State
**Current Task:** `None — Phase 4 planning (this report)`
**Active Boundary:** `Boundary E — IN PROGRESS` (QA-01 Pass B pending; QA-02/03 pending); Boundaries F–H planned, not started
**Next Actionable:** `TASK-SH-02` (print proof needs running API+Desktop+DB) + AuthController live tests (needs running API+JWT). All Phase-4 nodes DONE except SH-02; worked on `main`.
