# ⚽ SportEase: Sports Field Booking Platform

**SportEase** is a web application for **booking sports fields** (football, basketball, tennis, volleyball). Players search for available fields, book time slots, and manage their reservations. Field owners (admins) manage their fields, confirm or reject bookings, and follow statistics on their dashboard. Notifications are pushed in **real time with SignalR**.

Built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQL Server**. Testing uses unit tests (NUnit, xUnit) and test management with **Kiwi TCMS**.

---

## ✨ Features

### 👤 Players
- Register, log in, and edit your profile (session-based auth, passwords hashed with **BCrypt**)
- Browse and **search fields** by sport, city, date and time, price range, capacity, lighting, and parking
- View field details and **available time slots**
- **Book** a slot, view **My Bookings**, and **cancel** (subject to the cancellation deadline)
- Personal dashboard

### 🛠️ Admins (field owners)
- Dashboard with statistics: reservations and **revenue per month**
- **CRUD for fields**, including image upload (jpg/png/gif, up to 5 MB)
- Manage reservations: **confirm** or **reject** with a reason

### ⚡ Real time
- A SignalR hub (`/reservationHub`) notifies admins of new bookings and notifies players when their booking is confirmed or rejected

### 📏 Business rules (`appsettings.json → AppSettings`)
| Rule | Default |
|---|---|
| Max active reservations per user | 3 |
| Booking duration | 1–3 hours |
| Book up to | 30 days ahead |
| Cancellation deadline | 24 hours before the slot |
| Slot interval | 60 minutes |

Reservation statuses: `Pending`, `Confirmed`, `Cancelled`, `Completed`.

## 🏗️ Architecture

Layered MVC with the repository and service patterns, wired up with dependency injection:

```
Controllers  →  Services (business rules)  →  Repositories  →  EF Core DbContext  →  SQL Server
     │                                                                  
     └── Views (Razor) + SignalR hub
```

```
SportEase/
├── Controllers/        # Account, Terrain, Reservation, Admin, Home
├── Services/           # AuthService, TerrainService, ReservationService, StatisticsService
├── Repositories/       # User, Terrain, Reservation repositories (+ interfaces)
├── Data/               # ApplicationDbContext (+ seed data)
├── Models/
│   ├── Entities/       # User, Terrain, Reservation
│   └── ViewModels/
├── Hubs/               # ReservationHub (SignalR)
├── Helpers/            # FileUploadHelper
├── Views/              # Razor views
├── wwwroot/            # CSS, JS, images, uploads
└── SportEase.Tests/    # xUnit + Moq + FluentAssertions tests
TestSportEase/          # NUnit + Moq service tests
kiwi-tcms/              # Kiwi TCMS (Docker) + Python test-report generators
SportEase.sln
```

## 🛠️ Tech stack

| | |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 (SQL Server) |
| Database | SQL Server Express (local) / Azure SQL |
| Real time | SignalR |
| Validation | FluentValidation |
| Security | BCrypt.Net-Next, sessions |
| Tests | NUnit, xUnit, Moq, FluentAssertions, EF Core InMemory, coverlet |
| Test management | Kiwi TCMS (Docker), Python + openpyxl Excel reports |

## 🚀 Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server Express** (instance `localhost\SQLEXPRESS`) or another SQL Server instance
- Optional: Visual Studio 2022

### 1. Configure the database

The default connection string in `SportEase/appsettings.json` uses Windows authentication:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SportEaseDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
```

Change it if your SQL Server is elsewhere. The database is **created automatically** at startup (`EnsureCreated`).

### 2. Run

```bash
git clone https://github.com/fidaaltf58/SportEase.git
cd SportEase
dotnet restore
dotnet run --project SportEase
```

Then open <http://localhost:5017>, or <https://localhost:7128> with the `https` profile.

### Demo accounts (seeded)

| Role | Email | Password |
|---|---|---|
| Admin | `admin@sportease.com` | `Admin@123` |
| User | `user@sportease.com` | `User@123` |

> Change these credentials before any public deployment.

## 🧪 Testing

```bash
# NUnit service tests
dotnet test TestSportEase

# xUnit tests (unit + integration)
dotnet test SportEase/SportEase.Tests
```

### Kiwi TCMS

Test plans, test cases, and bug reports are managed in **Kiwi TCMS**:

```bash
cd kiwi-tcms
docker compose up -d        # → http://localhost:8090
```

The Python scripts in `kiwi-tcms/` (`generate_full_report.py`, ...) build the Excel test reports (`Rapport_Tests_Complet_SportEase.xlsx`):

```bash
pip install pandas openpyxl
python kiwi-tcms/generate_full_report.py
```

## Author

**Fidaa Letaief** · [@fidaaltf58](https://github.com/fidaaltf58)
