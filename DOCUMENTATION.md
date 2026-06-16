# Event Booking System — Setup & Run Documentation

**Course:** CSE 6234 Software Design (Term 2610) · **Group:** 09 (TT5L)
**Project:** E-Commerce Event Booking System
**Language/Stack:** C# · .NET 8 · ASP.NET Core MVC · Entity Framework Core · SQLite · Bootstrap 5

This document contains everything needed to set up, run, demo, and troubleshoot the project on any Windows machine. (macOS/Linux notes at the end.)

---

## 1. Prerequisites

| Requirement | Version | Purpose |
|---|---|---|
| .NET SDK | 8.0 or later | Build and run the app |
| Web browser | Any modern | Use the app |
| Internet (first run only) | — | NuGet package restore + Bootstrap CDN |

No database server is needed — the app uses a local SQLite file created automatically.

### 1.1 Install the .NET 8 SDK

Pick **one** option:

**Option A — Official installer (recommended for presentation laptops)**
1. Go to https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **SDK** (not "Runtime") for Windows x64.
3. Run the installer, accept defaults.

**Option B — winget (Windows 10/11)**
```powershell
winget install Microsoft.DotNet.SDK.8
```

**Option C — No-admin install (user folder)**
```powershell
Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "$env:TEMP\dotnet-install.ps1" -UseBasicParsing
& "$env:TEMP\dotnet-install.ps1" -Channel 8.0 -InstallDir "$env:USERPROFILE\.dotnet"
# Then, in every new terminal before running dotnet commands:
$env:PATH = "$env:USERPROFILE\.dotnet;" + $env:PATH
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
```

**Verify the install** (open a NEW terminal after installing):
```powershell
dotnet --version
```
Expected output: `8.0.xxx` (any 8.x or later is fine).

---

## 2. Running the Project

### 2.1 Quick start

```powershell
cd "D:\Event Booking System\EventBookingSystem"
dotnet run
```

Wait for:
```
Now listening on: http://localhost:5000
```

Open your browser at: **http://localhost:5000**

> First run takes longer — NuGet restores packages and EF Core creates + seeds the database (`eventbooking.db`). Subsequent runs start in seconds.

### 2.2 Stop the server

Press `Ctrl + C` in the terminal running `dotnet run`.

### 2.3 Run on a different port

```powershell
$env:ASPNETCORE_URLS = "http://localhost:8080"
dotnet run
```

### 2.4 Reset the database (fresh demo data)

Stop the server, then:
```powershell
cd "D:\Event Booking System\EventBookingSystem"
Remove-Item eventbooking.db* -Force
dotnet run
```
The database is recreated and reseeded automatically on startup.

---

## 3. Demo Accounts (seeded automatically)

| Role | Email | Password | Lands on |
|---|---|---|---|
| **Admin** | admin@ebs.com | admin123 | Admin dashboard |
| **Organizer** | leen@ebs.com | organizer123 | My Events |
| **Organizer** | hanan@ebs.com | organizer123 | My Events |
| **Customer** | fatima@ebs.com | customer123 | Browse Events |
| **Customer** | zenab@ebs.com | customer123 | Browse Events |

**Seeded events:** Rock Concert 2026 (Axiata Arena), Tech Summit 2026 (MMU Cyberjaya), Food Festival (Pavilion KL) — each with ticket categories, prices, and quantities.

New Customer and Organizer accounts can also be self-registered from the **Sign Up** page. Admin registration is intentionally blocked.

---

## 4. Demo Walkthrough (suggested presentation flow)

### Customer journey (≈3 min)
1. Login as `fatima@ebs.com` / `customer123`.
2. **Browse Events** → use the search box / category filter.
3. Open **Rock Concert 2026** → **Book Now**.
4. Select ticket quantities (e.g. 2× VIP).
5. Choose a payment method — this demonstrates the **Strategy Pattern**:
   - **Credit Card**: enter any 12+ digit number, e.g. `4111111111111111`
   - **Online Banking**: pick a bank
   - **E-Wallet**: enter any wallet ID, e.g. `EW-123`
6. **Confirm & Pay** → confirmation page shows booking ref, seats, payment ref.
7. Click the **bell icon** → notifications created by the **Observer Pattern**.
8. **My Bookings** → view history, optionally **Cancel** (restores ticket stock + marks payment Refunded).

### Organizer journey (≈2 min)
1. Logout → login as `leen@ebs.com` / `organizer123`.
2. Dashboard shows event/booking/revenue stats.
3. **New Event** → create an event with ticket categories.
4. **Bookings** → see the customer's booking from step above.
5. **Reports** → per-event sales table.
6. Bell icon → organizer received an Observer notification for the new booking.

### Admin journey (≈2 min)
1. Logout → login as `admin@ebs.com` / `admin123`.
2. Dashboard → system-wide KPIs (users, bookings, gross revenue, failed payments).
3. **Users** → deactivate any account (deactivated users cannot log in), reactivate it.
4. **All Bookings** → full transaction monitor.
5. Bell icon → audit-log notifications from the Observer Pattern.

### Error-handling demos (optional, shows robustness)
- Checkout with an invalid card (e.g. `12`) → payment fails, booking is cancelled, **ticket stock is restored** (transaction rollback).
- Submit checkout with 0 tickets → validation message.
- Wrong password at login → friendly error.
- Register with an existing email → rejected.

---

## 5. Architecture Summary

### Three layers (assignment requirement)

```
┌────────────────────────────────────────────────┐
│ UI LAYER          Views/ (Razor) + Controllers/│
├────────────────────────────────────────────────┤
│ BUSINESS LOGIC    Services/ + Patterns/        │
│                   (Auth, Event, Reporting,     │
│                    Factory, Strategy,          │
│                    Observer, Facade)           │
├────────────────────────────────────────────────┤
│ DATABASE LAYER    Repositories/ + Data/ +      │
│                   Models/ → SQLite             │
└────────────────────────────────────────────────┘
```

### Design patterns and where to find them

| Pattern | File(s) | What it does here |
|---|---|---|
| **Factory Method** | `Patterns/Factory/UserFactory.cs` | Creates Customer / Organizer / Admin objects from a role; used by registration. |
| **Strategy** | `Patterns/Strategy/IPaymentStrategy.cs`, `PaymentStrategies.cs`, `PaymentContext.cs` | Interchangeable payment algorithms (Credit Card / Online Banking / E-Wallet); checkout never changes when a method is added. |
| **Observer** | `Patterns/Observer/IBookingObserver.cs`, `BookingSubject.cs`, `Observers.cs` | Booking status changes notify Customer, Organizer, and Admin observers, which persist notifications. |
| **Facade** | `Patterns/Facade/BookingFacade.cs` | One `ProcessFullBooking()` call orchestrates availability check → ticket reservation → payment (Strategy) → confirmation → notifications (Observer), inside a DB transaction. |

### Module map (matches report Section 2)

| Module | Implementation |
|---|---|
| User Management | `AuthService`, `UserFactory`, `AccountController` |
| Event Management | `EventService`, `OrganizerController`, `EventsController` |
| Booking | `BookingFacade`, `BookingsController` |
| Payment | `PaymentContext` + strategies (inside facade flow) |
| Notification | `BookingSubject` + 3 observers, bell page |
| Reporting | `ReportingService`, organizer/admin report pages |

---

## 6. Project Structure

```
D:\Event Booking System\
├── README.md                 ← project overview
├── DOCUMENTATION.md          ← this file
├── e2e-test.ps1              ← automated end-to-end test script
└── EventBookingSystem\       ← the application
    ├── Program.cs            ← DI registration, session, DB seed
    ├── appsettings.json      ← SQLite connection string
    ├── EventBookingSystem.csproj
    ├── eventbooking.db       ← SQLite DB (auto-created)
    ├── Data\
    │   ├── AppDbContext.cs   ← EF Core context (TPH inheritance for users)
    │   └── DbSeeder.cs       ← demo accounts + events
    ├── Models\               ← User, Event, Booking, Ticket, Payment, Notification
    ├── Patterns\
    │   ├── Factory\
    │   ├── Strategy\
    │   ├── Observer\
    │   └── Facade\
    ├── Repositories\         ← data-access interfaces + EF implementations
    ├── Services\             ← AuthService, EventService, ReportingService,
    │                            PasswordHasher, SessionExtensions
    ├── Controllers\          ← Home, Account, Events, Bookings, Organizer, Admin
    ├── Views\                ← Razor pages per controller + shared _Layout
    └── wwwroot\css\site.css  ← custom styling
```

---

## 7. Automated Tests

A 40-check end-to-end test script is included. With the app running on port 5000:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Event Booking System\e2e-test.ps1"
```

It covers: public pages, search, 404s, login/logout, bad credentials, full checkout with all 3 payment strategies, failed-payment rollback (stock restoration), zero-ticket validation, booking cancellation, registration (factory), duplicate-email rejection, admin-signup blocking, organizer CRUD + reports, admin dashboards + user management, Observer notifications for all 3 roles, and role-based access control.

Latest result: **40/40 PASS**.

> Note: the test script writes data into the database (a test user, a test event, bookings). Reset the DB afterwards (Section 2.4) if you want pristine demo data.

---

## 8. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `No .NET SDKs were found` | Only the .NET *runtime* is installed | Install the **SDK** (Section 1.1) |
| `dotnet: command not found` | PATH not refreshed | Open a **new** terminal; or set PATH per Option C |
| `Failed to bind to address ... 5000` | Port already in use | Use another port (Section 2.3) or kill the other process: `Get-Process -Name dotnet | Stop-Process` |
| Browser shows old/odd data | Stale DB from previous experiments | Reset the DB (Section 2.4) |
| `database is locked` | Two app instances sharing one SQLite file | Stop all instances, run only one |
| Page styles missing | No internet (Bootstrap is CDN-loaded) | Connect to internet, or download Bootstrap into `wwwroot/lib` and update `_Layout.cshtml` links |
| Login works but everything logs out | Cookies blocked | Allow cookies for localhost (session auth uses a cookie) |

---

## 9. Running on macOS / Linux

Same steps; only the SDK install differs:

```bash
# macOS
brew install dotnet-sdk

# Ubuntu/Debian
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

# Then:
cd "Event Booking System/EventBookingSystem"
dotnet run
```

SQLite and ASP.NET Core are fully cross-platform; no code changes required.

---

## 10. Submission Checklist Mapping (assignment brief)

| Requirement | Where satisfied |
|---|---|
| Working prototype with UI layer | Razor views + Bootstrap (`Views/`, `wwwroot/`) |
| Business Logic Layer | `Services/` + `Patterns/` |
| Database Layer | EF Core + SQLite (`Data/`, `Repositories/`, `Models/`) |
| Software design patterns | Factory Method, Strategy, Observer, Facade (all in running code, not just slides) |
| Role-based access | Customer / Organizer / Admin with guarded controllers |
| Instructions to lecturer | This file (DOCUMENTATION.md) + README.md |
