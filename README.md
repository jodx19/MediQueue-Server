# 🏥 MediQueue EMR — Clinic Management System

> A production-ready Electronic Medical Records (EMR) system built with **.NET 9**, **Angular 18**, and **Clean Architecture**.

---

## 📐 Architecture

MediQueue follows **Clean Architecture** (also known as Onion Architecture), enforcing strict dependency rules:

```
MediQueue.Domain          ← Core (no external dependencies)
    ↑
MediQueue.Application     ← Use cases (MediatR, FluentValidation, DTOs)
    ↑
MediQueue.Infrastructure  ← Technical implementations (EF Core, Caching, etc.)
    ↑
MediQueue.API             ← Presentation (Controllers, SignalR Hubs, Middleware)
    ↑
MediQueue.Server.Host     ← Composition Root (Program.cs only)
```

---

## 🏗️ Project Structure

```
MediQueue EMR Clinic System/
├── MediQueue.Server/       ← .NET 9 Backend
│   ├── MediQueue.Domain/         # Core Entities & Logic
│   ├── MediQueue.Application/    # Use cases & DTOs
│   ├── MediQueue.Infrastructure/ # Persistence & Services
│   ├── MediQueue.API/            # Controllers & Middleware
│   └── MediQueue.Server.Host/    # Startup Project
└── MediQueue.Client/       ← Angular 18 Frontend
```

---

## 🚀 Technology Stack

| Category | Technology |
|---|---|
| **Runtime** | .NET 9 |
| **Web Framework** | ASP.NET Core 9 |
| **ORM** | Entity Framework Core 9 (SQL Server) |
| **CQRS / Mediator** | MediatR 14 |
| **Validation** | FluentValidation 11 |
| **Caching** | MemoryCache (Dev) / Redis (Production Ready) |
| **Real-time** | SignalR |
| **Authentication** | JWT Bearer (ASP.NET Core Identity) |
| **Logging** | Serilog (Console logging enabled) |
| **Frontend** | Angular 18 |

---

## 🛠️ Getting Started (Backend)

### 1. Configuration (`appsettings.json`)
Ensure your connection string is set in `MediQueue.Server.Host/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MediQueueDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 2. Database Setup
```bash
# From the root directory
cd MediQueue.Server/MediQueue.Server.Host
dotnet ef database update --project ../MediQueue.Infrastructure --startup-project .
```

### 3. Running the App
```bash
dotnet run --launch-profile "http"
```
API available at: `http://localhost:5055/swagger`

---

## 🛠️ Getting Started (Frontend)

### 1. Install Dependencies
```bash
cd MediQueue.Client
npm install
```

### 2. Generate API Client
```bash
npm run generate:api
```

### 3. Run the App
```bash
npm start
```
App available at: `http://localhost:4200`

---

## 📡 Key Endpoints

| URL | Description |
|---|---|
| `/swagger` | API Documentation |
| `/health` | Health Check (Database status) |
| `/hubs/clinic` | SignalR WebSocket hub |

---

*Built with ❤️ — Clean Architecture, SOLID principles, and a focus on scalability.*
