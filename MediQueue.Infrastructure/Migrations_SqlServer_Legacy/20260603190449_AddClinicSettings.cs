using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClinicPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    WorkEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AppointmentDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowOnlineBooking = table.Column<bool>(type: "bit", nullable: false),
                    RequireDepositForBooking = table.Column<bool>(type: "bit", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicSettings");
        }
    }
}
