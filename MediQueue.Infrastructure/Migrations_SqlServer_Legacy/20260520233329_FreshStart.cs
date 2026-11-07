using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FreshStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId1",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DoctorId",
                table: "Users",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Doctors_DoctorId",
                table: "Users",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Doctors_DoctorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DoctorId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId1",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId1",
                table: "Appointments",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId1",
                table: "Appointments",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId1",
                table: "Appointments",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
