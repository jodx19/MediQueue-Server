using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MediQueue.Application.Auth.DTOs;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;
using MediQueue.Infrastructure.ExternalServices;

namespace MediQueue.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IPasswordHasher<AppUser>> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;
    private readonly CancellationToken _ct = CancellationToken.None;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();
        _passwordHasherMock = new Mock<IPasswordHasher<AppUser>>();
        _tokenServiceMock = new Mock<ITokenService>();

        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s["ExpiryMinutes"]).Returns("60");
        _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _configurationMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    private static AppUser CreateTestUser(string role = "Admin", bool isActive = true)
    {
        var user = AppUser.Create("testuser", "test@mediqueue.com", "Test", "User", "01012345678", "hashed_password", Enum.Parse<UserRole>(role));
        if (!isActive) user.Deactivate();
        return user;
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = CreateTestUser();
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync("test@mediqueue.com"))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, "CorrectPass"))
            .Returns(PasswordVerificationResult.Success);
        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns("jwt-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        var request = new LoginRequestDto("test@mediqueue.com", "CorrectPass");
        var result = await _authService.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailure()
    {
        var user = CreateTestUser();
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync("test@mediqueue.com"))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, "WrongPass"))
            .Returns(PasswordVerificationResult.Failed);

        var request = new LoginRequestDto("test@mediqueue.com", "WrongPass");
        var result = await _authService.LoginAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnFailure()
    {
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync("unknown@test.com"))
            .ReturnsAsync((AppUser?)null);

        var request = new LoginRequestDto("unknown@test.com", "AnyPass");
        var result = await _authService.LoginAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnFailure()
    {
        var inactiveUser = CreateTestUser(role: "Admin", isActive: false);
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync("inactive@test.com"))
            .ReturnsAsync(inactiveUser);

        var request = new LoginRequestDto("inactive@test.com", "AnyPass");
        var result = await _authService.LoginAsync(request);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokenPair()
    {
        var user = CreateTestUser();
        var refreshTokenField = typeof(AppUser).GetProperty("RefreshToken");
        refreshTokenField?.SetValue(user, "valid-refresh-token");
        var expiryField = typeof(AppUser).GetProperty("RefreshTokenExpiryTime");
        expiryField?.SetValue(user, DateTime.UtcNow.AddDays(7));

        _unitOfWorkMock.Setup(u => u.Users.GetByRefreshTokenAsync("valid-refresh-token"))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns("new-jwt-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

        var request = new RefreshTokenRequestDto("old-jwt", "valid-refresh-token");
        var result = await _authService.RefreshTokenAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("new-jwt-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnFailure()
    {
        var user = CreateTestUser();
        var refreshTokenField = typeof(AppUser).GetProperty("RefreshToken");
        refreshTokenField?.SetValue(user, "expired-refresh-token");
        var expiryField = typeof(AppUser).GetProperty("RefreshTokenExpiryTime");
        expiryField?.SetValue(user, DateTime.UtcNow.AddDays(-1));

        _unitOfWorkMock.Setup(u => u.Users.GetByRefreshTokenAsync("expired-refresh-token"))
            .ReturnsAsync(user);

        var request = new RefreshTokenRequestDto("old-jwt", "expired-refresh-token");
        var result = await _authService.RefreshTokenAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithEmptyToken_ShouldReturnFailure()
    {
        var request = new RefreshTokenRequestDto("old-jwt", "");
        var result = await _authService.RefreshTokenAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Refresh token is required");
    }

    [Fact]
    public async Task PatientLoginAsync_WithValidMrnAndDob_ShouldReturnToken()
    {
        var patient = Patient.Register(
            new PersonName("Patient", "", "Test"),
            new DateOnly(1990, 1, 15),
            Gender.Male,
            BloodType.OPos,
            "12345678901234",
            new ContactInfo("01012345678"),
            new MediQueue.Domain.ValueObjects.Address("St", "City", "Gov", "Egypt"),
            MaritalStatus.Single);

        _unitOfWorkMock.Setup(u => u.Patients.GetByMRNAsync("MRN-001"))
            .ReturnsAsync(patient);

        var user = CreateTestUser(role: "Patient");
        _unitOfWorkMock.Setup(u => u.Users.GetByPatientIdAsync(patient.Id))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns("patient-jwt");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("patient-refresh");

        var result = await _authService.PatientLoginAsync("MRN-001", new DateTime(1990, 1, 15));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("patient-jwt");
    }

    [Fact]
    public async Task PatientLoginAsync_WithWrongDob_ShouldReturnFailure()
    {
        var patient = Patient.Register(
            new PersonName("Patient", "", "Test"),
            new DateOnly(1990, 1, 15),
            Gender.Male,
            BloodType.OPos,
            "12345678901234",
            new ContactInfo("01012345678"),
            new MediQueue.Domain.ValueObjects.Address("St", "City", "Gov", "Egypt"),
            MaritalStatus.Single);

        _unitOfWorkMock.Setup(u => u.Patients.GetByMRNAsync("MRN-001"))
            .ReturnsAsync(patient);

        var result = await _authService.PatientLoginAsync("MRN-001", new DateTime(1990, 6, 15));

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task PatientLoginAsync_WithUnknownMrn_ShouldReturnFailure()
    {
        _unitOfWorkMock.Setup(u => u.Patients.GetByMRNAsync("MRN-UNKNOWN"))
            .ReturnsAsync((Patient?)null);

        var result = await _authService.PatientLoginAsync("MRN-UNKNOWN", DateTime.UtcNow);

        result.IsSuccess.Should().BeFalse();
    }
}
