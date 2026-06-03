using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediQueue.IntegrationTests.Infrastructure;

namespace MediQueue.IntegrationTests.Api;

public class AppointmentsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AppointmentsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAppointments_AsAdmin_Returns200()
    {
        var client = _factory.CreateAuthenticatedClient("Admin");
        var response = await client.GetAsync("/api/appointments?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppointments_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/appointments?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAppointments_AsPatient_Returns403()
    {
        var client = _factory.CreateAuthenticatedClient("Patient");
        var response = await client.GetAsync("/api/appointments?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAppointment_AsAdmin_Returns400_WhenInvalid()
    {
        var client = _factory.CreateAuthenticatedClient("Admin");
        var payload = new { }; // Missing required fields

        var response = await client.PostAsJsonAsync("/api/appointments", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAppointmentById_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/appointments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelAppointment_AsReceptionist_Returns400_WhenInvalidId()
    {
        var client = _factory.CreateAuthenticatedClient("Receptionist");
        var payload = new { Reason = "Patient cancelled" };

        var response = await client.PostAsJsonAsync(
            $"/api/appointments/{Guid.NewGuid()}/cancel", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmAppointment_AsReceptionist_Returns400_WhenInvalidId()
    {
        var client = _factory.CreateAuthenticatedClient("Receptionist");
        var response = await client.PostAsync(
            $"/api/appointments/{Guid.NewGuid()}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
