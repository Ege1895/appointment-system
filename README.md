# 🗓️ Appointment System (SaaS)

A robust, multi-tenant SaaS appointment booking system built with **.NET 10**. This platform provides a modular monolith architecture designed for scalability, featuring multi-business support, employee scheduling, and real-time appointment management.

---

## 🚀 Tech Stack

- **Framework:** .NET 10.0 (ASP.NET Core Web API)
- **Primary Database:** PostgreSQL (using Entity Framework Core)
- **Caching:** Redis (StackExchange.Redis)
- **Logging:** Serilog with Console and Elasticsearch sinks
- **Validation:** FluentValidation (Automatic ASP.NET integration)
- **Security:** JWT Authentication (RS256/HS256 compatible), BCrypt Password Hashing
- **Containerization:** Docker & Docker Compose
- **Reporting:** ClosedXML (Excel) & CsvHelper (CSV)

---

## ✨ Key Features

### 🏢 Multi-Tenant Business Management
- **Centralized Management:** Support for multiple businesses and branches.
- **Custom Offerings:** Define services, prices, and durations per business.
- **Working Hours:** Highly configurable scheduling for businesses and individual employees.

### 📅 Appointment Lifecycle
- **Slot-based Booking:** Concurrency-safe slot calculations.
- **State Management:** Automated state transitions (Scheduled -> InProgress -> Completed/Cancelled).
- **Reminders:** Background workers for automated appointment reminders.
- **Messaging:** Integrated messaging and private notes for appointments.

### 👥 User & Employee Modules
- **Unified Authentication:** Single entry point for Businesses, Staff, and Customers.
- **Employee Scheduling:** Individual shift management and availability tracking.
- **Reporting:** Export appointment and performance data to Excel or CSV.

### 🛠️ Infrastructure & Security
- **Modular Monolith:** Clean separation of concerns via internal modules.
- **Rate Limiting:** Protect sensitive endpoints (Login, Refresh Token, Export).
- **Resilience:** Health checks for API, DB, and Redis.
- **Architecture:** Middleware-based exception handling and auditable entity tracking.

---

## 🏗️ Architecture

The project follows a **Modular Monolith** pattern. Each feature is encapsulated within the `Modules/` directory:

- `AppointmentModule`: Booking logic and background workers.
- `Auth`: Unified authentication and token management.
- `BusinessModule`: Business profiles and settings.
- `EmployeeModule`: Staff management.
- `OfferingModule`: Service definitions and pricing.
- `ReportingModule`: Data export and analytics.
- `Shared/`: Core utilities (Authentication, Caching, Middleware, Database Interceptors).

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Redis](https://redis.io/)
- *Optional:* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Local Development

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Ege1895/appointment-system.git
   cd appointment-system
   ```

2. **Configure Environment:**
   Copy `.env.example` to `.env` and update the values.
   ```bash
   cp .env.example .env
   ```

3. **Database Setup:**
   Migrations are applied automatically on startup. Ensure your PostgreSQL connection string in `.env` or `appsettings.json` is correct.

4. **Run the Application:**
   ```bash
   dotnet run --project AppointmentSystem.Api.csproj
   ```
   The API will be available at `http://localhost:8080` (or as configured in `ASPNETCORE_URLS`).

### Docker Setup (Recommended)

Run the entire stack (API, PostgreSQL, Redis) with a single command:

```bash
docker-compose up -d
```

- **API:** `http://localhost:8080`
- **Swagger UI:** `http://localhost:8080/swagger`
- **PostgreSQL:** `localhost:5433`
- **Redis:** `localhost:6379`

---

## 🧪 Testing

The project uses **xUnit** for unit testing. To run the tests:

```bash
dotnet test
```

Tests are located in the `AppointmentSystem.Tests/` directory, covering core services like `AppointmentService`, `ReportingService`, and `BusinessCustomerService`.

---

## 📜 API Documentation

Once the application is running, you can explore the API using Swagger:
👉 [http://localhost:8080/swagger](http://localhost:8080/swagger)

---

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.
