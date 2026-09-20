# TECH_STACK.md
### HospitalProject — runtimes, packages, and commands (version-pinned, verified against csproj on 2026-09-20)

> Every version below was read from the **actual `.csproj` files** in this repo before being written. There is **no package centralization** (`Directory.Packages.props` absent); each `csproj` pins versions inline. There is **no solution-level `.sln`** — only `HospitalProject.slnx` (XML solution; the newer `.slnx` format).

---

## 1. Runtimes / SDK

| Item | Version | Where declared |
|---|---|---|
| .NET SDK | **8.0.x** (SDK 8.0.400+ — builds `net8.0`/`net8.0-windows` targets) | `global.json`? **none present**; CLI discovers latest 8.0 |
| `Hospital.Core` | `net8.0` | `Hospital.Core.csproj` |
| `Hospital.API` | `net8.0` | `Hospital.API.csproj` |
| `Hospital.Desktop` | `net8.0-windows` (WPF WinExe) | `Hospital.Desktop.csproj` |
| `HospitalProject.slnx` | XML solution (3 projects, **no classic .sln**) | repository root |
| Tooling | `dotnet-ef` (8.0.x) — via `Microsoft.EntityFrameworkCore.Tools` (PrivateAssets) | `Hospital.API.csproj` |

---

## 2. Packages — `Hospital.Core` (`Hospital.Core.csproj`)
| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Extensions.Identity.Stores` | **8.0.10** | Identity stores in the shared layer (base `IdentityUser`/`IdentityRole` contracts) |
| `Serilog.AspNetCore` | **8.0.0** | Referenced (shared logging surface) — **Serilog is not configured in Core**; logging wiring lives in API `Program.cs` |

## 3. Packages — `Hospital.API` (`Hospital.API.csproj`)
| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | **8.0.10** | JWT bearer validation (Scheme in `Program.cs`) |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | **8.0.10** | Identity + EF stores (`AddIdentity` + `AddEntityFrameworkStores`) |
| `Microsoft.EntityFrameworkCore.Design` | **8.0.10** | Design-time (`dotnet ef`) — PrivateAssets=all |
| `Microsoft.EntityFrameworkCore.SqlServer` | **8.0.10** | SQL Server provider (`UseSqlServer`) |
| `Microsoft.EntityFrameworkCore.Tools` | **8.0.10** | EF CLI migrations — PrivateAssets=all |
| `Serilog.AspNetCore` | **8.0.0** | Serilog host (Console + rolling `Logs/log-.txt`) |
| `Swashbuckle.AspNetCore` | **6.6.2** | Swagger/OpenAPI + JWT Bearer security definition |

## 4. Packages — `Hospital.Desktop` (`Hospital.Desktop.csproj`)
| Package | Version | Purpose | Actually used? |
|---|---|---|---|
| `CommunityToolkit.Mvvm` | **8.4.1** | MVVM toolkit | ❌ **Unused** — repo uses hand-rolled `BaseViewModel`/`RelayCommand` |
| `Microsoft.Extensions.Http` | **8.0.0** | `HttpClient` factory / typed client | ✅ `ApiService` uses `AddHttpClient` / typed client |
| `Newtonsoft.Json` | **13.0.4** | JSON (DTO binding + `DateOnlyJsonConverter`) | ✅ |
| `Serilog.AspNetCore` | **8.0.0** | Serilog | ⚠️ Referenced, **Desktop does not initialize Serilog** (API-only mill) |
| `System.Security.Cryptography.ProtectedData` | **8.0.0** | DPAPI `ProtectedData` for `EncryptionHelper` (SavedPassword) | ✅ |

---

## 5. Databases
| Item | Value |
|---|---|
| Engine | **SQL Server** (LocalDB via `DefaultConnection` — see gitignored `appsettings.json`) |
| Database name (seed script) | `HospitalManagementDB` (from `Hospital.sql`) |
| ORM | **EF Core 8.0.10** (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Migrations | In `Hospital.API/Migrations/` — **9** applied + `ApplicationDbContextModelSnapshot.cs`; last = `20260402181244_newfeature` |
| Seeding | `DbSeeder.SeedRolesAndAdminAsync` at API startup (Admin/Manager/User + admin user); `DbSeeder` also seeds NightShiftTeam rows |

---

## 6. Build / Run Commands (canonical)

```bash
# Build the whole solution (XML slnx — the only entry point)
dotnet build HospitalProject.slnx

# Run the API (startup project) — https://localhost:7278, Swagger at /swagger
dotnet run --project Hospital.API

# Run the Desktop (WPF) — net8.0-windows, WPF MVVM
dotnet run --project Hospital.Desktop

# EF migrations — run FROM Hospital.API/ (csproj has EF Tools/Database Design)
cd Hospital.API
dotnet ef migrations add <Name>          # add migration
dotnet ef migrations list                # list applied/pending
dotnet ef database update                # apply to LocalDB
```

---

## 7. No / Not Implemented
- **No** `Directory.Packages.props` (versions are inline per csproj).
- **No** `global.json` SDK pin.
- **No** classic `.sln` (only `.slnx`).
- **No** test project, CI (`dotnet run` only), `.editorconfig`, README, LICENSE, Dockerfile.
- **No** repository/UoW layer (controllers use `ApplicationDbContext` directly).
- **No** DI container in Desktop (VMs call `new ApiService()`).
- **Serilog initialized only in API**; Desktop references Serilog package but has no Serilog bootstrap (pending task).
