<h1 align="center">
  <br>
  🏥 MediQueue EMR — Backend Server
  <br>
</h1>

<p align="center">
  <b>Multi-Specialty Electronic Medical Records System</b><br/>
  Built with ASP.NET Core 9 · Clean Architecture · CQRS · Domain-Driven Design
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/EF%20Core-9-blue?style=for-the-badge" />
  <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver" />
  <img src="https://img.shields.io/badge/SignalR-Real--Time-00aaff?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Hangfire-Jobs-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Redis-Cache-DC382D?style=for-the-badge&logo=redis" />
  <img src="https://img.shields.io/badge/Azure-Blob%20Storage-0078D4?style=for-the-badge&logo=microsoftazure" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker" />
</p>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Solution Structure](#-solution-structure)
- [Domain Layer](#-domain-layer)
- [Application Layer](#-application-layer)
- [Infrastructure Layer](#-infrastructure-layer)
- [API Layer](#-api-layer)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Reference](#-api-reference)
- [Database](#-database)
- [Real-Time & Background Jobs](#-real-time--background-jobs)
- [Security](#-security)
- [Docker](#-docker)

---

## 🏥 Overview

MediQueue is a **production-grade, multi-specialty clinic EMR system** designed to digitise the complete patient lifecycle — from registration to clinical visit, prescription, lab requests, invoicing, and payment settlement.

The backend is an **ASP.NET Core 9 REST API** with a real-time SignalR hub, background job processing, distributed caching, cloud file storage, and email notifications — all running in Docker.

### Core Capabilities

| Feature | Description |
|---|---|
| Patient Management | Registration, MRN generation, medical history, allergies, chronic conditions |
| Doctor Management | Profiles, specialisations, working shifts, availability calendar |
| Appointment Booking | Conflict detection, priority, multi-type scheduling |
| Clinical Visits (EMR) | SOAP notes, vital signs, diagnoses, prescriptions, lab & imaging requests, referrals, procedures |
| Invoicing & Billing | Multi-item invoices, discount application, payment recording, revenue reports |
| Authentication | JWT Bearer, role-based access (Admin / Doctor / Receptionist) |
| Real-Time | SignalR hub for live notifications |
| Background Jobs | Daily revenue reports via Hangfire |
| File Storage | Medical attachments via Azure Blob Storage |
| Caching | Redis-backed distributed cache |
| Observability | Serilog structured logging, health checks (SQL + Redis + Hangfire) |

---

## 🏛 Architecture

MediQueue strictly follows **Clean Architecture** with a unidirectional dependency rule:

```
API → Application → Domain
         ↑
  Infrastructure (implements contracts)
```

Each layer has a single responsibility and zero knowledge of the layers above it.

```
┌─────────────────────────────────────────────────────────┐
│                        MediQueue.API                    │
│  Controllers · Middleware · SignalR Hub · Program.cs    │
├─────────────────────────────────────────────────────────┤
│                   MediQueue.Application                 │
│  Commands · Queries · Handlers · Behaviors · DTOs       │
│  MediatR CQRS · AutoMapper · FluentValidation           │
├─────────────────────────────────────────────────────────┤
│                    MediQueue.Domain                     │
│  Entities · ValueObjects · Events · Interfaces · Enums  │
│  Pure C# — zero external dependencies                   │
├─────────────────────────────────────────────────────────┤
│                 MediQueue.Infrastructure                │
│  EF Core · Repositories · Services · Hangfire · Redis   │
│  SignalR · Azure Blob · Email · JWT · Migrations        │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns Used

| Pattern | Where |
|---|---|
| **CQRS** | Every use-case is a Command or Query dispatched via MediatR |
| **Repository** | Each aggregate root has a dedicated typed repository interface |
| **Unit of Work** | `IUnitOfWork` wraps all repositories under a single transaction |
| **Result<T>** | All handlers return `Result<T>` — no exceptions for business failures |
| **Domain Events** | Entities raise events (e.g. `PatientRegisteredEvent`) consumed by handlers |
| **Pipeline Behaviors** | Validation → Logging → Performance wraps every MediatR request |
| **Specification** | Queries encapsulate filter logic, preventing fat repositories |
| **Value Objects** | Immutable domain concepts: `Money`, `PersonName`, `Address`, `MedicalCode` |

---

## 📁 Solution Structure

```
MediQueue.Server/
├── MediQueue.Domain/
│   ├── Entities/           # 15 rich domain entities
│   ├── ValueObjects/       # 9 immutable value types
│   ├── Events/             # 8 domain event files
│   ├── Enums/              # 19 domain enumerations
│   ├── Exceptions/         # 8 domain exception types
│   └── Interfaces/         # 8 repository + UoW contracts
│
├── MediQueue.Application/
│   ├── Patients/           # 6 commands, 4 queries
│   ├── Doctors/            # 5 commands, 4 queries
│   ├── Appointments/       # 3 commands, 2 queries
│   ├── ClinicalVisits/     # 10 commands, 4 queries
│   ├── Invoices/           # 5 commands, 3 queries
│   ├── Dashboard/          # 2 queries
│   ├── Behaviors/          # Validation, Logging, Performance
│   ├── Common/             # Result<T>, ICommand, IQuery, PagedResult
│   └── Contracts/          # IAuthService, IEmailService, ICacheService, etc.
│
├── MediQueue.Infrastructure/
│   ├── Persistence/
│   │   ├── Context/        # ClinicDbContext (7 DbSets)
│   │   ├── Configurations/ # 7 EF Fluent API configs
│   │   ├── Repositories/   # 7 concrete repositories
│   │   └── Migrations/     # EF Core migrations
│   ├── ExternalServices/   # Auth, Email, Redis, Azure Blob, Hangfire, SignalR
│   └── Hubs/               # ClinicHub (SignalR)
│
└── MediQueue.API/
    ├── Controllers/        # 8 REST controllers
    ├── Middleware/         # GlobalExceptionMiddleware
    └── Program.cs          # Full DI composition root
```

---

## 🧬 Domain Layer

The **Domain layer is the heart** of the system. It contains pure business logic with zero infrastructure dependencies.

### Entities

All entities extend a base `Entity` class with domain event support.

| Entity | Aggregate Root | Key Behaviour |
|---|---|---|
| `Patient` | ✅ | Generates MRN, manages allergies/conditions, raises `PatientRegisteredEvent` |
| `Doctor` | ✅ | Manages working shifts, specialisations, raises `DoctorUnavailableEvent` |
| `Appointment` | ✅ | Conflict-detection guard, status machine, raises booking events |
| `ClinicalVisit` | ✅ | SOAP workflow, hosts prescriptions/labs/diagnoses/referrals |
| `Invoice` | ✅ | Multi-item billing, discount logic, payment settlement, raises `InvoiceEvents` |
| `AppUser` | ✅ | Identity entity — ties to ASP.NET Core Identity |
| `MedicalAttachment` | ✅ | File metadata, links to clinical visit or patient |
| `Allergy` | ❌ | Child of Patient |
| `ChronicCondition` | ❌ | Child of Patient |
| `ClinicalNote` | ❌ | Child of ClinicalVisit, raises `ClinicalNoteCreatedEvent` |
| `Diagnosis` | ❌ | Child of ClinicalVisit |
| `DoctorQualification` | ❌ | Child of Doctor |
| `ImagingRequest` | ❌ | Child of ClinicalVisit |
| `Referral` | ❌ | Child of ClinicalVisit |
| `MedicalProcedure` | ❌ | Child of ClinicalVisit |

### Value Objects

Immutable, equality-by-value types. EF Core maps them as owned entities:

| Value Object | Properties | Purpose |
|---|---|---|
| `PersonName` | FirstName, LastName | Patient/Doctor name |
| `Address` | Street, City, Country | Location |
| `ContactInfo` | Phone, Email | Contact details |
| `Money` | Amount, Currency | Financial values |
| `MedicalCode` | Code, Description, System | ICD/CPT coding |
| `PrescriptionItem` | Medication, Dosage, Frequency, Duration | Prescription line |
| `VitalSign` | Type, Value, Unit, RecordedAt | Clinical measurement |
| `WorkingShift` | DayOfWeek, Start, End | Doctor schedule |
| `WorkingHourSlot` | StartTime, EndTime | Granular slot |

### Domain Events

Domain events decouple side effects from business logic:

| Event | Raised By | Consumer |
|---|---|---|
| `PatientRegisteredEvent` | Patient.Register() | Sends welcome email |
| `PatientDeactivatedEvent` | Patient.Deactivate() | Cancels pending appointments |
| `DoctorCreatedEvent` | Doctor.Create() | Sends onboarding notification |
| `DoctorUnavailableEvent` | Doctor.SetUnavailable() | Notifies affected patients |
| `AppointmentEvents` | Appointment aggregate | Reminder scheduling |
| `ClinicalVisitEvents` | ClinicalVisit aggregate | Triggers invoice creation |
| `ClinicalNoteCreatedEvent` | ClinicalNote | Audit logging |
| `InvoiceEvents` | Invoice aggregate | Payment confirmation email |

---

## ⚙️ Application Layer

The Application layer orchestrates use-cases using the **CQRS + MediatR** pattern.

### How a Request Flows

```
HTTP Request
    ↓
Controller.Action()
    ↓
mediator.Send(new XxxCommand(...))
    ↓
[ValidationBehavior]  → FluentValidation runs; returns 400 on failure
    ↓
[LoggingBehavior]     → Logs request name + user
    ↓
[PerformanceBehavior] → Warns if handler > 500ms
    ↓
XxxCommandHandler.Handle()
    ↓
Repository / Domain Method
    ↓
unitOfWork.SaveChangesAsync()
    ↓
Result<T> returned to controller
```

### Commands by Module

**Patients:**
- `RegisterPatientCommand` — validates national ID uniqueness, assigns MRN, raises `PatientRegisteredEvent`
- `UpdatePatientCommand` — updates demographics and contact info
- `DeactivatePatientCommand` — soft-deletes, raises `PatientDeactivatedEvent`
- `AddAllergyCommand` / `RemoveAllergyCommand` — manages allergy list
- `AddChronicConditionCommand` — appends chronic condition

**Doctors:**
- `CreateDoctorCommand` — creates doctor profile with specialisation
- `UpdateDoctorCommand` — updates bio and contact
- `SetDoctorUnavailableCommand` — marks leave period, raises `DoctorUnavailableEvent`
- `AddWorkingShiftCommand` / `RemoveWorkingShiftCommand` — manages schedule

**Appointments:**
- `BookAppointmentCommand` — checks doctor availability and conflict, raises booking event
- `CancelAppointmentCommand` — transitions status, raises cancellation event
- `RescheduleAppointmentCommand` — validates new slot before rescheduling

**ClinicalVisits (EMR core):**
- `CreateClinicalVisitCommand` — opens visit linked to appointment
- `UpdateSOAPNoteCommand` — saves Subjective, Objective, Assessment, Plan
- `AddVitalSignCommand` — records measurement (BP, HR, Temp, etc.)
- `AddDiagnosisCommand` — attaches ICD code
- `CreatePrescriptionCommand` — adds prescription line items
- `AddLabRequestCommand` — creates lab test request
- `AddImagingRequestCommand` — creates radiology/imaging request
- `AddReferralCommand` — creates specialist referral with urgency
- `AddProcedureCommand` — records billable clinical procedure
- `FinalizeClinicalVisitCommand` — closes visit, triggers invoice creation

**Invoices:**
- `CreateInvoiceCommand` — generates invoice from visit procedures
- `AddInvoiceItemCommand` — appends line item (service/medication/procedure)
- `ApplyDiscountCommand` — applies percentage or fixed discount
- `RecordPaymentCommand` — records payment method and amount
- `CancelInvoiceCommand` — voids invoice

### Pipeline Behaviors

| Behavior | Logic |
|---|---|
| `ValidationBehavior<T>` | Discovers all FluentValidation validators for the request type; aggregates errors into `Result.Failure` |
| `LoggingBehavior<T>` | Logs `[Request]` and `[Response]` with request name, user ID, and elapsed time |
| `PerformanceBehavior<T>` | Logs a warning if handler execution exceeds 500 ms |

### Result Pattern

All handlers return `Result` or `Result<T>` — never throw for business failures:

```csharp
// Success
return Result<PatientDto>.Success(dto);

// Business failure — no exception thrown
return Result.Failure("Patient with this National ID already exists.");
```

The controller maps `Result.IsSuccess` → `200 OK` or `Result.Error` → `400/404`.

---

## 🔧 Infrastructure Layer

Implements all contracts defined in Application and Domain layers.

### ClinicDbContext

`ClinicDbContext : IdentityDbContext<AppUser>` with 7 DbSets:

```
Patients | Doctors | Appointments | ClinicalVisits | Invoices | Users | Attachments
```

Plus all ASP.NET Core Identity tables (AspNetRoles, AspNetUserRoles, etc.).

### EF Core Configurations (Fluent API)

| Config File | Key Mappings |
|---|---|
| `PatientConfiguration` | MRN unique index, owned Address + ContactInfo, JSON column for allergies list |
| `DoctorConfiguration` | Owned PersonName, JSON working shifts, specialty enum conversion |
| `AppointmentConfiguration` | FK constraints to Patient + Doctor, status enum, composite index on (DoctorId, ScheduledAt) |
| `ClinicalVisitConfiguration` | Owned VitalSigns collection, prescription item table split, owned SOAP note |
| `InvoiceConfiguration` | Owned Money values, invoice items table, payment record |
| `MedicalHistoryConfiguration` | Chronic conditions and medication tables |
| `AppUserConfiguration` | Identity extension fields (role, linked DoctorId) |

### Repositories

Each repository implements the domain interface using EF Core:

| Repository | Domain Interface | Key Methods |
|---|---|---|
| `PatientRepository` | `IPatientRepository` | GetByMRN, SearchByName, GetWithHistory |
| `DoctorRepository` | `IDoctorRepository` | GetBySpecialty, GetWithSchedule, GetAvailableSlots |
| `AppointmentRepository` | `IAppointmentRepository` | GetConflicting, GetByPatient, GetByDoctor |
| `ClinicalVisitRepository` | `IClinicalVisitRepository` | GetByAppointment, GetPatientHistory |
| `InvoiceRepository` | `IInvoiceRepository` | GetByPatient, GetRevenueSummary |
| `MedicalAttachmentRepository` | `IMedicalAttachmentRepository` | GetByEntity |
| `UserRepository` | `IUserRepository` | GetByEmail, GetByRole |

### External Services

| Service | Implementation | Technology |
|---|---|---|
| `IAuthService` | `AuthService` | ASP.NET Core Identity + JWT |
| `IEmailService` | `EmailNotificationService` | MailKit / SMTP |
| `ICacheService` | `RedisCacheService` | StackExchange.Redis |
| `IStorageService` | `AzureBlobStorageService` | Azure.Storage.Blobs |
| `ISchedulerService` | `HangfireSchedulerService` | Hangfire |
| `IRealtimeService` | `SignalRRealtimeService` | ASP.NET Core SignalR |

---

## 🌐 API Layer

### Controllers

| Controller | Route Prefix | Responsibility |
|---|---|---|
| `AuthController` | `/api/auth` | Login, register user, refresh token |
| `PatientsController` | `/api/patients` | Full CRUD + medical history |
| `DoctorsController` | `/api/doctors` | CRUD + schedule management |
| `AppointmentsController` | `/api/appointments` | Booking, cancellation, rescheduling |
| `ClinicalVisitsController` | `/api/clinical-visits` | EMR workflow endpoints |
| `InvoicesController` | `/api/invoices` | Billing and payment |
| `DashboardController` | `/api/dashboard` | Clinic stats and revenue reports |
| `AttachmentsController` | `/api/attachments` | Upload/download medical files |

### Middleware

**`ApiResponseFilter`** — A global action filter that intercepts all successful responses and wraps them in a standard `ApiResponse<T>` format, ensuring the frontend always receives a consistent structure:

```json
{
  "isSuccess": true,
  "data": { ... },
  "message": "Operation completed successfully.",
  "errors": null
}
```

**`GlobalExceptionMiddleware`** — catches all unhandled exceptions and maps them to structured JSON responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Patient with this National ID already exists.",
  "traceId": "00-abc123..."
}
```

Exception mapping:
- `DomainException` → 400 Bad Request
- `NotFoundException` → 404 Not Found
- `ValidationException` → 422 Unprocessable Entity
- Unhandled → 500 Internal Server Error

### SignalR Hub

`ClinicHub` at `/hubs/clinic`:

| Method | Direction | Purpose |
|---|---|---|
| `NotifyAppointmentBooked` | Server → Client | Broadcast new appointment |
| `NotifyVisitStarted` | Server → Client | Notify doctor when patient arrives |
| `NotifyInvoicePaid` | Server → Client | Confirm payment to receptionist |

---

## 🛠 Tech Stack

| Category | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core 9 |
| ORM | Entity Framework Core 9 |
| Database | Microsoft SQL Server 2022 |
| CQRS | MediatR 12 |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Auth | ASP.NET Core Identity + JwtBearer |
| Real-Time | ASP.NET Core SignalR |
| Background Jobs | Hangfire + SQL Server storage |
| Cache | Redis (StackExchange.Redis) |
| File Storage | Azure Blob Storage |
| Email | MailKit (SMTP) |
| Logging | Serilog + File sink |
| API Docs | Swagger / OpenAPI 3 |
| Health Checks | AspNetCore.Diagnostics.HealthChecks |
| Containerisation | Docker + docker-compose |

---

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Docker Desktop (optional — for SQL Server, Redis)
- SQL Server instance (or use docker-compose)

### 1. Clone

```bash
git clone https://github.com/jodx19/MediQueue-Server.git
cd MediQueue-Server
```

### 2. Configure

Edit `MediQueue.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MediQueueDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR-256-BIT-SECRET-KEY-CHANGE-THIS",
    "Issuer": "MediQueue",
    "Audience": "MediQueueClient",
    "ExpiryMinutes": 60
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "AzureBlobStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "mediqueue-attachments"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@mediqueue.com",
    "SenderName": "MediQueue EMR"
  }
}
```

### 3. Apply Migrations

```bash
cd MediQueue.API
dotnet ef database update --project ../MediQueue.Infrastructure
```

### 4. Run

```bash
dotnet run --project MediQueue.API
```

API is available at `https://localhost:7000`

Swagger UI: `https://localhost:7000/swagger`

Health check: `https://localhost:7000/health`

Hangfire dashboard: `https://localhost:7000/hangfire`

---

## 📖 API Reference

### Auth

```
POST /api/auth/login       → { email, password } → { token, refreshToken, expiresAt }
POST /api/auth/register    → { name, email, password, role }
POST /api/auth/refresh     → { refreshToken } → { token }
```

### Patients

```
POST   /api/patients              → RegisterPatientCommand
GET    /api/patients/{id}         → GetPatientByIdQuery
GET    /api/patients/mrn/{mrn}    → GetPatientByMRNQuery
PUT    /api/patients/{id}         → UpdatePatientCommand
DELETE /api/patients/{id}         → DeactivatePatientCommand
GET    /api/patients/{id}/history → GetPatientMedicalHistoryQuery
GET    /api/patients/search?q=    → SearchPatientsQuery
POST   /api/patients/{id}/allergies        → AddAllergyCommand
DELETE /api/patients/{id}/allergies/{aid}  → RemoveAllergyCommand
POST   /api/patients/{id}/conditions       → AddChronicConditionCommand
```

### Doctors

```
POST   /api/doctors                     → CreateDoctorCommand
GET    /api/doctors                     → GetAllDoctorsQuery
GET    /api/doctors/{id}                → GetDoctorByIdQuery
PUT    /api/doctors/{id}                → UpdateDoctorCommand
GET    /api/doctors/specialty/{s}       → GetDoctorsBySpecialtyQuery
GET    /api/doctors/{id}/availability   → GetDoctorAvailabilityQuery
POST   /api/doctors/{id}/unavailable    → SetDoctorUnavailableCommand
POST   /api/doctors/{id}/shifts         → AddWorkingShiftCommand
DELETE /api/doctors/{id}/shifts/{sid}   → RemoveWorkingShiftCommand
```

### Appointments

```
POST   /api/appointments              → BookAppointmentCommand
GET    /api/appointments/{id}         → GetAppointmentByIdQuery
PUT    /api/appointments/{id}/cancel  → CancelAppointmentCommand
PUT    /api/appointments/{id}/reschedule → RescheduleAppointmentCommand
```

### Clinical Visits (EMR)

```
POST   /api/clinical-visits                          → CreateClinicalVisitCommand
GET    /api/clinical-visits/{id}                     → GetClinicalVisitByIdQuery
GET    /api/clinical-visits/appointment/{apptId}     → GetVisitByAppointmentQuery
GET    /api/clinical-visits/patient/{patientId}      → GetPatientClinicalHistoryQuery
PUT    /api/clinical-visits/{id}/soap                → UpdateSOAPNoteCommand
POST   /api/clinical-visits/{id}/vitals              → AddVitalSignCommand
POST   /api/clinical-visits/{id}/diagnoses           → AddDiagnosisCommand
POST   /api/clinical-visits/{id}/prescriptions       → CreatePrescriptionCommand
GET    /api/clinical-visits/patient/{id}/prescriptions → GetPatientPrescriptionsQuery
POST   /api/clinical-visits/{id}/lab-requests        → AddLabRequestCommand
POST   /api/clinical-visits/{id}/imaging-requests    → AddImagingRequestCommand
POST   /api/clinical-visits/{id}/referrals           → AddReferralCommand
POST   /api/clinical-visits/{id}/procedures          → AddProcedureCommand
PUT    /api/clinical-visits/{id}/finalize            → FinalizeClinicalVisitCommand
```

### Invoices

```
POST   /api/invoices                      → CreateInvoiceCommand
GET    /api/invoices/{id}                 → GetInvoiceByIdQuery
GET    /api/invoices/patient/{patientId}  → GetPatientInvoicesQuery
POST   /api/invoices/{id}/items           → AddInvoiceItemCommand
PUT    /api/invoices/{id}/discount        → ApplyDiscountCommand
POST   /api/invoices/{id}/payment         → RecordPaymentCommand
DELETE /api/invoices/{id}                 → CancelInvoiceCommand
```

### Dashboard

```
GET /api/dashboard/stats    → GetClinicStatsQuery  (today's patients, revenue, appts)
GET /api/dashboard/revenue  → GetRevenueReportQuery (date-range revenue breakdown)
```

---

## 🗄 Database

### Schema Overview

```
AspNetUsers ────────────────────────────────────────┐
                                                    │
Patients ──┬── Allergies                            │
           ├── ChronicConditions                    │
           ├── CurrentMedications                   │
           └── MedicalAttachments                   │
                                                    │
Doctors ───┬── DoctorQualifications                 │
           └── WorkingShifts (JSON)                 │
                                                    │
Appointments ──(Patient FK + Doctor FK)─────────────┘
           │
ClinicalVisits ─┬── ClinicalNotes
                ├── Diagnoses
                ├── PrescriptionItems
                ├── VitalSigns
                ├── LabRequests
                ├── ImagingRequests
                ├── Referrals
                └── MedicalProcedures

Invoices ──┬── InvoiceItems
           └── Payments
```

### Migrations

| Migration | Date | Description |
|---|---|---|
| `20260430135327_InitialMediQueueDb` | 2026-04-30 | Full initial schema |

Run migrations:
```bash
dotnet ef database update --project MediQueue.Infrastructure --startup-project MediQueue.API
```

---

## ⚡ Real-Time & Background Jobs

### SignalR Hub — `/hubs/clinic`

Connect from Angular:
```typescript
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/clinic', { accessTokenFactory: () => token })
  .withAutomaticReconnect()
  .build();

connection.on('AppointmentBooked', (data) => { ... });
connection.on('VisitStarted',      (data) => { ... });
connection.on('InvoicePaid',       (data) => { ... });
```

### Hangfire Background Jobs

| Job Name | Schedule | Action |
|---|---|---|
| `daily-revenue-report` | `Cron.Daily` | Computes daily revenue and sends summary email to admin |
| `invoice-overdue-checker` | `Cron.Daily` | Scans for unpaid invoices past due date and marks them overdue |
| `missed-appointment-processor` | `Cron.Minutely` | Scans for un-attended appointments past their scheduled time and marks them as No-Show |

*Note: All background jobs dispatch MediatR commands, completely decoupling the Hangfire infrastructure from the Entity Framework context.*

Hangfire Dashboard: `/hangfire` (Admin only in production)

---

## 🔒 Security

### JWT Authentication

- Token issued on login: contains `userId`, `email`, `role` claims
- All controllers require `[Authorize]` by default
- Role-based policies: `Admin`, `Doctor`, `Receptionist`

### Role Permissions Matrix

| Endpoint | Admin | Doctor | Receptionist |
|---|---|---|---|
| Patient CRUD | ✅ | 🔍 Read | ✅ |
| Doctor CRUD | ✅ | ❌ | ❌ |
| Book Appointment | ✅ | ❌ | ✅ |
| Clinical Visit (Write) | ❌ | ✅ | ❌ |
| Invoice (Write) | ✅ | ❌ | ✅ |
| Dashboard | ✅ | ❌ | ❌ |
| Hangfire | ✅ | ❌ | ❌ |

---

## 🐳 Docker

A `docker-compose.yml` at the solution root starts the full stack:

```bash
docker-compose up -d
```

Services started:

| Service | Port | Description |
|---|---|---|
| `mediqueue-api` | 7000 | ASP.NET Core API |
| `sqlserver` | 1433 | SQL Server 2022 |
| `redis` | 6379 | Redis cache |

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

<p align="center">
  Built with ❤️ using Clean Architecture · DDD · CQRS
</p>
