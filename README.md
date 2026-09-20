# HospitalProject — نظام إدارة المستشفى (Hospital Management System)

> **Arabic-first, RTL hospital HR / shift-management system.** .NET 8 monorepo: WPF desktop client + ASP.NET Core Web API + shared Core library.

---

## 🏗️ Solution Overview

| Item             | Value                                                                                                                                                                                    |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Solution         | `HospitalProject.slnx` (XML `.slnx` — the **only** solution entry point; no classic `.sln`)                                                                                              |
| Target framework | `net8.0` / `net8.0-windows` (Desktop/WPF)                                                                                                                                                |
| Database         | SQL Server (LocalDB, `HospitalManagementDB`) — **EF Core 8.0.10** via `ApplicationDbContext`                                                                                             |
| API endpoint     | `https://localhost:7278` (Swagger at `/swagger`)                                                                                                                                         |
| Docs             | [`PROJECT_MAP.md`](PROJECT_MAP.md) (navigation) · [`ARCHITECTURE.md`](ARCHITECTURE.md) (blueprint) · [`TECH_STACK.md`](TECH_STACK.md) (versions) · [`TASK_TREE.md`](TASK_TREE.md) (plan) |

### Projects

| Project             | Type                            | Role                                             |
| ------------------- | ------------------------------- | ------------------------------------------------ |
| `Hospital.Core/`    | class library (`net8.0`)        | Domain models, DTOs, enums — shared contracts    |
| `Hospital.API/`     | ASP.NET Core Web API (`net8.0`) | REST backend, JWT auth, EF data access, auditing |
| `Hospital.Desktop/` | WPF (`net8.0-windows`)          | MVVM desktop client (Arabic UI, RTL)             |

---

## 🚀 Getting Started

```bash
# 1. Restore + build the whole solution (canonical entry)
dotnet build HospitalProject.slnx

# 2. Run the API (startup project) — Swagger at https://localhost:7278/swagger
dotnet run --project Hospital.API

# 3. Run the Desktop client (WPF) — login with a seeded Admin account
dotnet run --project Hospital.Desktop
```

> **Prerequisites:** .NET SDK 8.0.400+, SQL Server LocalDB. The API seeds roles/Admin on startup (`DbSeeder`) and applies migrations via EF.

### EF Migrations (run from `Hospital.API/`)

```bash
cd Hospital.API
dotnet ef migrations list
dotnet ef database update
```

---

## 🔐 Authentication & Roles

- JWT Bearer; all endpoints require auth globally (`RequireAuthenticatedUser`), plus per-action `[Authorize(Roles=…)]`.
- Seeded roles: **Admin**, **Manager**, **User** (`Hospital.API/Data/DbSeeder.cs`).
- Login flow: `POST api/Auth/Login` → JWT stored in `ApiService._token` → authenticated HTTP calls from the Desktop client.

---

## 🧬 Feature Domains

Employees · Departments · JobTitles · Leaves (أجازات) · Absents (غيابات) · Transfers (التنقلات) · AuditLogs (سجلات التدقيق) · NightShiftTeams / ShiftSettings (نوبات ليلية) · Users/Auth.

See [`PROJECT_MAP.md`](PROJECT_MAP.md) §2.2–2.3 for the full controller/ViewModel/View inventory.

---

## 🧭 Repository Conventions (read before editing)

- **Soft-delete everywhere** — every domain entity has `isDeleted` with a global query filter; use `IgnoreQueryFilters()` deliberately to read soft-deleted rows. No hard deletes.
- **Arabic-first** — UI text, labels, messages, and API error `{message}` strings are Arabic; Desktop XAML is RTL.
- **DateOnly** — entities use `DateOnly`; Desktop uses a `DateOnly` DTO/JSON converter.
- **mvvm** — Desktop uses hand-rolled MVVM (`BaseViewModel`, `RelayCommand`). Do **not** switch to `CommunityToolkit.Mvvm` even though the package is referenced.
- **DTO names** - inconsistent casing (mixed `DTO`/`Dto`) exists **by design**; treat them as the API contract and do not "fix". (Historical: `JobTitleVeiwDTO` fixed to `JobTitleViewDTO` on 2026-09-20.)
- **One HTTP client** — Desktop uses a single `Hospital.Desktop/Services/ApiService.cs` (hardcoded base URL `https://localhost:7278/api/`).

> The full working contract is in [`AGENTS.md`](AGENTS.md) and [`rules.md`](rules.md). Read them before changing code.

---

## 📁 Documentation Set

| File                                      | Purpose                                                 |
| ----------------------------------------- | ------------------------------------------------------- |
| `PROJECT_MAP.md`                          | Verified navigation map of the whole codebase           |
| `ARCHITECTURE.md`                         | Blueprint: layers, data flow, key implementation points |
| `TECH_STACK.md`                           | Version-pinned package matrix + build commands          |
| `AGENTS.md` · `rules.md` · `.cursorrules` | Workflow contract & coding style                        |
| `TASK_TREE.md`                            | Production-readiness execution plan (gap-driven)        |

---

## 🧩 Known Gaps / Not Implemented

The following are **not present** in this repo (documented as gaps, not fixed silently):

- **No tests** (no test project, `dotnet test` has nothing to run)
- **No CI** — `.github/` exists but empty (no workflow)
- **No `README.md` history** — added this session
- **No `.editorconfig`**, **no `LICENSE`** — pending
- **No `/weatherforecast/` endpoint** — `Hospital.API.http` references a stub that does not exist
- ~258 nullable-reference warnings on `dotnet build` (existing, not treated as errors)

---

## 📄 License

Not selected (this repository has no `LICENSE` file; see `TASK_TREE.md` → `TASK-INF-03`).
