# MediQueue EMR Clinic System

A comprehensive, enterprise-grade Electronic Medical Records (EMR) and Clinic Management System built with modern technologies and following Clean Architecture principles.

## 🏗️ Architecture Overview

This project follows a **monorepo structure** with complete separation between frontend and backend:

```
MediQueue.Client/     # Angular 19 Frontend
MediQueue.Server/     # .NET 9 Backend API
├── MediQueue.Domain/
├── MediQueue.Application/
├── MediQueue.Infrastructure/
└── MediQueue.API/
```

## 🚀 Quick Start

### Prerequisites

- **.NET 9.0 SDK**
- **Node.js 18+**
- **Angular CLI 19**
- **SQL Server** (or compatible database)

### Backend Setup (.NET 9)

1. Navigate to the backend directory:
```bash
cd MediQueue.Server
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update database connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=MediQueueEMR;Trusted_Connection=true;"
  }
}
```

4. Run database migrations:
```bash
dotnet ef database update --project MediQueue.Infrastructure --startup-project MediQueue.API
```

5. Start the API server:
```bash
dotnet run --project MediQueue.API
```

The API will be available at: `https://localhost:7000`

### Frontend Setup (Angular 19)

1. Navigate to the frontend directory:
```bash
cd MediQueue.Client
```

2. Install dependencies:
```bash
npm install
```

3. Update API endpoint in `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/api'
};
```

4. Start the development server:
```bash
ng serve
```

The frontend will be available at: `http://localhost:4200`

## 📁 Project Structure

### Backend (MediQueue.Server)

#### 🏛️ Domain Layer (`MediQueue.Domain`)
- **Pure domain logic** with no external dependencies
- **Entities**: Patient, MedicalHistory, DentalChart, etc.
- **Value Objects**: DentalChart, specialized medical values
- **Domain Events**: PatientRegistered, AppointmentScheduled
- **Exceptions**: Domain-specific business rule violations
- **Interfaces**: Repository contracts, domain services

#### 🎯 Application Layer (`MediQueue.Application`)
- **Feature-based organization** (CQRS pattern)
- **Commands**: RegisterPatient, UpdatePatient, etc.
- **Queries**: GetPatientById, SearchPatients, etc.
- **DTOs**: Data transfer objects for API responses
- **Validators**: FluentValidation rules
- **Common**: Behaviors, mappings, exceptions

#### 🔧 Infrastructure Layer (`MediQueue.Infrastructure`)
- **Persistence**: Entity Framework Core, repositories
- **External Services**: Email, file storage, notifications
- **Caching**: Redis/Memory cache implementation
- **Configuration**: Service registration, settings

#### 🌐 API Layer (`MediQueue.API`)
- **Controllers/Endpoints**: RESTful API endpoints
- **Middleware**: Authentication, logging, exception handling
- **Swagger**: API documentation
- **Configuration**: Startup, dependency injection

### Frontend (MediQueue.Client)

#### 🎨 Design System
- **Apple-style/Minimalist UI** approach
- **Clean typography** and spacing
- **Consistent color palette** and components
- **Responsive design** for all devices

#### 📱 Folder Structure
- **Core/**: Shared services, guards, interceptors
- **Shared/**: Common components, models, utilities
- **Features/**: Feature modules (Patients, Appointments, etc.)
- **Environments/**: Environment-specific configurations

## 🛠️ Technology Stack

### Backend
- **.NET 9** - Latest .NET framework
- **Entity Framework Core 9** - ORM and database access
- **MediatR** - CQRS and mediator pattern
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation
- **SQL Server** - Primary database (configurable)

### Frontend
- **Angular 19** - Modern frontend framework
- **TypeScript** - Type-safe JavaScript
- **RxJS** - Reactive programming
- **Angular Material** - UI components (optional)
- **RxJS** - Reactive programming
- **SCSS** - Styling with Apple-inspired design

## 🏥 Core Features

### Patient Management
- ✅ Complete patient registration with demographics
- ✅ Medical history tracking
- ✅ Dental chart integration
- ✅ Appointment scheduling
- ✅ Document management
- ✅ Insurance information

### Clinical Workflow
- 🔄 Medical records management
- 🔄 Lab results tracking
- 🔄 Prescription management
- 🔄 Clinical notes and documentation

### Administrative
- 🔄 Staff management
- 🔄 Clinic configuration
- 🔄 Reporting and analytics
- 🔄 Audit trails

## 🔐 Security Features

- **JWT Authentication** for secure access
- **Role-based access control** (RBAC)
- **Data encryption** for sensitive information
- **Audit logging** for compliance
- **Input validation** and sanitization

## 🐳 Docker Support

The project is structured for easy containerization:

```bash
# Build and run with Docker Compose
docker-compose up -d
```

Docker configuration files are included for:
- **Backend API** (.NET 9 runtime)
- **Frontend** (Nginx serving Angular build)
- **Database** (SQL Server or PostgreSQL)

## 📊 Development Workflow

### Code Quality
- **Clean Architecture** principles
- **SOLID principles** adherence
- **Comprehensive unit testing** (xUnit)
- **Integration testing** for critical paths
- **Code coverage** requirements

### CI/CD Pipeline
- **GitHub Actions** for automated builds
- **Automated testing** on pull requests
- **Code quality checks** (SonarQube)
- **Automated deployment** to staging/production

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- 📧 Email: support@mediqueue.com
- 📱 Documentation: [docs.mediqueue.com](https://docs.mediqueue.com)
- 🐛 Issues: [GitHub Issues](https://github.com/your-org/MediQueue/issues)

## 🌟 Acknowledgments

- Built with ❤️ for healthcare professionals
- Following **Clean Architecture** best practices
- Inspired by modern **enterprise software patterns**
- Designed for **scalability** and **maintainability**

---

**MediQueue EMR Clinic System** - Empowering healthcare providers with modern technology.
