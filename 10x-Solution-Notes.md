# My 10x Solution - Mahmoud Mostafa El Safi

## Section 1 — The Problem

MediQueue solves the complex problem of multi-tenant clinic management for small and medium-sized dental and medical clinics. Managing multiple independent clinics (tenants) under a single platform often leads to data isolation issues, administrative overhead, and inefficient manual processes. 

The 10x impact of this system is directly seen in administrative automation. For example, replacing the manual, error-prone tracking of overdue invoices and missed appointments with automated daily background jobs. Instead of clinic staff spending hours cross-referencing records and manually flagging missed visits or unpaid bills, MediQueue automatically flips overdue invoices and processes missed appointments every night, saving significant administrative time and ensuring no revenue or patient follow-up falls through the cracks.

## Section 2 — How it's implemented

**API Endpoints**
The system exposes robust RESTful API endpoints to handle clinic operations, such as managing patients, appointments, and invoices. These endpoints use clean architecture principles and CQRS to ensure that each HTTP request is processed efficiently and mapped accurately to domain actions.

**Database**
Data is stored using Entity Framework Core with SQL Server, implementing a strict multi-tenant architecture. Every query is automatically filtered by a `TenantId` via Global Query Filters in the DbContext, guaranteeing that clinics can only ever access their own data.

**Authentication**
Security is handled via ASP.NET Core Identity and JWT Bearer tokens, configured within the dependency injection setup. This ensures that only authenticated staff and administrators can access the system, with roles enforcing permissions across different clinic endpoints.

**Background/Cron Jobs**
Background jobs run daily via Hangfire, automatically flipping overdue invoices and processing missed appointments. This was verified live via the Hangfire dashboard and a manual trigger that completed in 499ms, proving the reliability of the automated workflow.

**Caching**
To improve performance, MediQueue uses a distributed caching mechanism with Redis. Frequently accessed data, such as tenant configurations, are cached to drastically reduce database load and speed up API response times, which was verified by confirming cache hits.

**LLM Integration**
The system features an AI-powered drug interaction checker using Groq/OpenAI APIs. When doctors prescribe medications, this service analyzes the combination and warns them of potential adverse interactions, enhancing patient safety directly at the point of care.

### Steps to run the project

1. **Clone the repository**: Pull the latest code to your local machine.
2. **Restore and Build**: Run `dotnet restore` and `dotnet build` in the backend directory.
3. **Configure Settings**: Update the configuration files with your SQL Server connection string, Upstash Redis connection string, and Groq/OpenAI API key.
4. **Apply Migrations**: Run `dotnet ef database update` to create the schema and seed the initial data.
5. **Run the Backend**: Execute `dotnet run` to start the application.
6. **Log In**: Use the seeded admin credentials to log into the system via the generated Swagger interface or the client application.
