using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediQueue.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMediQueueDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSpecialty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AlternativePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConsultationFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsultationFeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FollowUpFeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FollowUpFeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkingShifts = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AlternativePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalRecordNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorQualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorQualifications_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    BloodPressure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "int", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: true),
                    OxygenSaturation = table.Column<int>(type: "int", nullable: true),
                    IsSmoker = table.Column<bool>(type: "bit", nullable: false),
                    SmokingPackYears = table.Column<int>(type: "int", nullable: true),
                    IsAlcoholConsumer = table.Column<bool>(type: "bit", nullable: false),
                    AlcoholConsumptionDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDrugUser = table.Column<bool>(type: "bit", nullable: false),
                    DrugUseDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExerciseFrequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DietType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasAllergies = table.Column<bool>(type: "bit", nullable: false),
                    AllergyDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HasMedicationAllergies = table.Column<bool>(type: "bit", nullable: false),
                    MedicationAllergyDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HasChronicConditions = table.Column<bool>(type: "bit", nullable: false),
                    ChronicConditionDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HasDiabetes = table.Column<bool>(type: "bit", nullable: false),
                    DiabetesType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasHypertension = table.Column<bool>(type: "bit", nullable: false),
                    HasHeartDisease = table.Column<bool>(type: "bit", nullable: false),
                    HasAsthma = table.Column<bool>(type: "bit", nullable: false),
                    FamilyHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SurgicalHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CurrentMedications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PastMedications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImmunizationHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastMenstrualPeriod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    PregnancyCount = table.Column<int>(type: "int", nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastExaminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExaminingPhysician = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Allergen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConditionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ICD10Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiagnosedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    PrescribedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientMedications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubjectiveNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObjectiveNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssessmentNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VitalSigns = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalVisits_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalVisits_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalVisits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoicePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicePayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitDiagnoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitDiagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitDiagnoses_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitImagingRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyPart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitImagingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitImagingRequests_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitLabRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitLabRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitLabRequests_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitPrescriptions_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitProcedures_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitReferrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferredToSpecialty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferredToDoctorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Urgency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitReferrals_ClinicalVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "ClinicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => new { x.PrescriptionId, x.Id });
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_VisitPrescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "VisitPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_ScheduledAt",
                table: "Appointments",
                columns: new[] { "DoctorId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalVisits_AppointmentId",
                table: "ClinicalVisits",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalVisits_DoctorId",
                table: "ClinicalVisits",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalVisits_PatientId",
                table: "ClinicalVisits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorQualifications_DoctorId",
                table: "DoctorQualifications",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_InvoiceId",
                table: "InvoicePayments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_Patient_ExamDate",
                table: "MedicalHistories",
                columns: new[] { "PatientId", "LastExaminationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PatientId",
                table: "MedicalHistories",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_PatientId",
                table: "PatientAllergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_PatientId",
                table: "PatientConditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_PatientId",
                table: "PatientMedications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MedicalRecordNumber",
                table: "Patients",
                column: "MedicalRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalId",
                table: "Patients",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitDiagnoses_VisitId",
                table: "VisitDiagnoses",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitImagingRequests_VisitId",
                table: "VisitImagingRequests",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitLabRequests_VisitId",
                table: "VisitLabRequests",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitPrescriptions_VisitId",
                table: "VisitPrescriptions",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitProcedures_VisitId",
                table: "VisitProcedures",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitReferrals_VisitId",
                table: "VisitReferrals",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "DoctorQualifications");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "InvoicePayments");

            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "PatientAllergies");

            migrationBuilder.DropTable(
                name: "PatientConditions");

            migrationBuilder.DropTable(
                name: "PatientMedications");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VisitDiagnoses");

            migrationBuilder.DropTable(
                name: "VisitImagingRequests");

            migrationBuilder.DropTable(
                name: "VisitLabRequests");

            migrationBuilder.DropTable(
                name: "VisitProcedures");

            migrationBuilder.DropTable(
                name: "VisitReferrals");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "VisitPrescriptions");

            migrationBuilder.DropTable(
                name: "ClinicalVisits");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
