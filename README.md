# E-Commerce Event Booking System

CSE 6234 Software Design — Group 09 (TT5L) prototype.

A full-stack C# / ASP.NET Core MVC web application demonstrating modular software design and four object-oriented design patterns: **Factory Method**, **Strategy**, **Observer**, and **Facade**.

## Architecture

Three-layer architecture mapped to ASP.NET Core MVC:

| Layer | Folders | Responsibilities |
|-------|---------|------------------|
| **UI Layer** | `Views/`, `Controllers/`, `wwwroot/` | Razor views, controllers, Bootstrap CSS |
| **Business Logic Layer** | `Services/`, `Patterns/` | Auth, Event, Booking, Payment, Notification, Reporting + design patterns |
| **Database Layer** | `Data/`, `Repositories/`, `Models/` | EF Core `AppDbContext`, repositories, entities, SQLite |

## Modules

- **User Management** — registration, login, role-based dashboards (Customer / Organizer / Admin).
- **Event Management** — organizers CRUD events, ticket categories, pricing, capacity.
- **Booking** — customers select tickets, system reserves and confirms.
- **Payment** — Strategy-based: Credit Card / Online Banking / E-Wallet.
- **Notification** — Observer-based: Customer, Organizer, and Admin receive updates.
- **Reporting** — sales summaries for organizers; system-wide KPIs for admin.

## Design Patterns

| Pattern | Where | Why |
|---------|-------|-----|
| **Factory Method** | `Patterns/Factory/UserFactory.cs` | Centralizes role-based user creation. |
| **Strategy** | `Patterns/Strategy/*.cs` | Swap payment algorithms without changing checkout. |
| **Observer** | `Patterns/Observer/*.cs` | Booking subject notifies multiple observers. |
| **Facade** | `Patterns/Facade/BookingFacade.cs` | One entry point orchestrates booking + payment + notification. |

## Tech Stack

- **C# / .NET 8**
- **ASP.NET Core MVC**
- **Entity Framework Core 8** with **SQLite**
- **Bootstrap 5** + Bootstrap Icons (CDN)
- Server-side session for auth

## Run Instructions

### 1. Install .NET 8 SDK

Download from https://dotnet.microsoft.com/download/dotnet/8.0 (currently only the runtime is installed on this machine — the SDK is required to build).

Verify after install:

```powershell
dotnet --version    # should print 8.x.x
```

### 2. Run the app

```powershell
cd "D:\Event Booking System\EventBookingSystem"
dotnet restore
dotnet run
```

Then open: http://localhost:5000

The SQLite database `eventbooking.db` is created automatically on first run and seeded with demo data.

### 3. Seeded Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@ebs.com | admin123 |
| Organizer | leen@ebs.com | organizer123 |
| Organizer | hanan@ebs.com | organizer123 |
| Customer | fatima@ebs.com | customer123 |
| Customer | zenab@ebs.com | customer123 |

Seeded events: *Rock Concert 2026*, *Tech Summit 2026*, *Food Festival*.

## Demo Walkthrough

1. **Login as customer** (`fatima@ebs.com`) → Browse Events → pick an event → Book Now.
2. **Checkout** — select quantities, pick payment method (test card e.g. `4111111111111111`), confirm.
3. **Confirmation** shows booking ref + payment ref.
4. **Notifications bell** shows entries created by the Observer pattern for customer.
5. **Logout** → log in as `leen@ebs.com` (organizer) → see the new booking under "Bookings" + notification.
6. **Logout** → log in as `admin@ebs.com` → dashboard shows aggregated stats + audit notifications.

## Project Layout

```
EventBookingSystem/
├── Program.cs                    DI setup, session, DB init
├── appsettings.json              SQLite connection string
├── EventBookingSystem.csproj
├── Data/
│   ├── AppDbContext.cs           EF Core context (TPH inheritance)
│   └── DbSeeder.cs               Demo data
├── Models/                       User, Event, Booking, Payment, Notification
├── Patterns/
│   ├── Factory/UserFactory.cs
│   ├── Strategy/                 IPaymentStrategy + 3 concrete + PaymentContext
│   ├── Observer/                 ISubject + 3 observers + BookingSubject
│   └── Facade/BookingFacade.cs
├── Repositories/                 User/Event/Booking/Notification repos
├── Services/                     Auth, Event, Reporting, PasswordHasher, SessionExt
├── Controllers/                  Home, Account, Events, Bookings, Organizer, Admin
├── Views/                        Razor pages (_Layout shared)
└── wwwroot/css/site.css          Custom styles
```

## Design Principles Applied

- **Abstraction** — `User` abstract base, `IPaymentStrategy`, `IBookingObserver`, repository interfaces.
- **Modularity** — six independent modules with clear boundaries.
- **High Cohesion** — each service handles one concern (e.g. `AuthService` only authenticates).
- **Low Coupling** — controllers depend on interfaces and services, not concrete EF types.
- **Encapsulation** — entity state mutation via service methods; repositories hide EF specifics.
- **Open-Closed** — new payment methods (Strategy) and new user roles (Factory) added without modifying clients.
- **Functional Independence** — patterns are isolated under `Patterns/` for clear demonstration.

## Group Members

- Leen Alsamman (1221308098)
- Hanan Esam (243UC245Y2)
- Zenab Hosameldin Karamalla

---
Built for CSE 6234 Software Design, Term 2610, MMU.
