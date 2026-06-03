using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediQueue.IntegrationTests.Infrastructure;

namespace MediQueue.IntegrationTests.Api;

public class AuthControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public AuthControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithMissingFields_ShouldReturn422()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var payload = new { Email = "admin@mediqueue.com", Password = "WrongPassword123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatientLogin_WithWrongDob_ShouldReturn401()
    {
        var payload = new { MRN = "MRN-001", DateOfBirth = "1990-01-01" };
        var response = await _client.PostAsJsonAsync("/api/auth/patient-login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturn401()
    {
        var payload = new { Token = "invalid-jwt", RefreshToken = "invalid-refresh" };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithAdminRole_ShouldReturn200()
    {
        var authClient = _factory.CreateAuthenticatedClient("Admin");
        var response = await authClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCurrentUser_WithPatientRole_ShouldReturn200()
    {
        var authClient = _factory.CreateAuthenticatedClient("Patient");
        var response = await authClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
