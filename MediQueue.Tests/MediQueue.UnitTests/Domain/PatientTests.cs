using FluentAssertions;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.UnitTests.Domain;

public class PatientTests
{
    private static Patient RegisterPatient()
    {
        var name = new PersonName("Ahmed", "M", "Ali");
        var contact = new ContactInfo("01012345678", "ahmed@test.com");
        var address = new Address("Street", "Cairo", "Cairo", "Egypt");

        return Patient.Register(
            name,
            new DateOnly(1990, 1, 15),
            Gender.Male,
            BloodType.OPos,
            "12345678901234",
            contact,
            address,
            MaritalStatus.Single);
    }

    [Fact]
    public void Register_ShouldSetPropertiesAndBeActive()
    {
        var patient = RegisterPatient();

        patient.PersonName.FullName.Should().Contain("Ahmed");
        patient.IsActive.Should().BeTrue();
        patient.MedicalRecordNumber.Should().StartWith("MRN-");
    }

    [Fact]
    public void Register_ShouldRaisePatientRegisteredEvent()
    {
        var patient = RegisterPatient();

        patient.DomainEvents.Should().ContainSingle(e => e is PatientRegisteredEvent);
    }

    [Fact]
    public void AddAllergy_ShouldAppendToAllergiesList()
    {
        var patient = RegisterPatient();

        patient.AddAllergy("Penicillin", AllergySeverity.Moderate, "Rash and swelling");

        patient.Allergies.Should().HaveCount(1);
        patient.Allergies.First().Allergen.Should().Be("Penicillin");
    }

    [Fact]
    public void RemoveAllergy_WhenExists_ShouldRemoveIt()
    {
        var patient = RegisterPatient();
        patient.AddAllergy("Penicillin", AllergySeverity.Mild, "Rash");
        var allergyId = patient.Allergies.First().Id;

        patient.RemoveAllergy(allergyId);

        patient.Allergies.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAllergy_WhenNotExists_ShouldNotThrow()
    {
        var patient = RegisterPatient();

        var act = () => patient.RemoveAllergy(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void AddChronicCondition_ShouldAppendToConditionsList()
    {
        var patient = RegisterPatient();

        patient.AddChronicCondition("Diabetes Type 2", new DateOnly(2020, 3, 1), "Controlled with metformin");

        patient.ChronicConditions.Should().HaveCount(1);
        patient.ChronicConditions.First().Name.Should().Be("Diabetes Type 2");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var patient = RegisterPatient();

        patient.Deactivate();

        patient.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaisePatientDeactivatedEvent()
    {
        var patient = RegisterPatient();
        patient.ClearDomainEvents();

        patient.Deactivate();

        patient.DomainEvents.Should().ContainSingle(e => e is PatientDeactivatedEvent);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldDoNothing()
    {
        var patient = RegisterPatient();
        patient.Deactivate();
        patient.ClearDomainEvents();

        patient.Deactivate();

        patient.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateContactInfo_ShouldReplaceContact()
    {
        var patient = RegisterPatient();
        var newContact = new ContactInfo("01112345678", "newemail@test.com");

        patient.UpdateContactInfo(newContact);

        patient.ContactInfo.Phone.Should().Be("01112345678");
    }
}
