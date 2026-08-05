using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.IntegrationTests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ClinicDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseInMemoryDatabase("MediQueueTestDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            db.Database.EnsureCreated();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKeyThatIsLongEnoughForHmacSha256!",
                ["JwtSettings:Issuer"] = "MediQueueTest",
                ["JwtSettings:Audience"] = "MediQueueTestClient",
                ["JwtSettings:ExpiryMinutes"] = "60",
            });
        });
    }

    public HttpClient CreateAuthenticatedClient(string role = "Admin")
    {
        var client = CreateClient();
        var token = GenerateTestToken(role);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateTestToken(string role)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKeyThatIsLongEnoughForHmacSha256!");
        var claims = new[]
        {
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Role, role),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Name, $"test-{role.ToLower()}@mediqueue.com"),
        };
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "MediQueueTest",
            audience: "MediQueueTestClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature));
        return tokenHandler.WriteToken(token);
    }
}
