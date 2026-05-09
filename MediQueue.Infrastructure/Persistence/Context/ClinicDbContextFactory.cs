using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.Persistence.Context;

public sealed class ClinicDbContextFactory : IDesignTimeDbContextFactory<ClinicDbContext>
{
    public ClinicDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "MediQueue.API");
        var basePath = Directory.Exists(apiProjectPath)
            ? apiProjectPath
            : Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MediQueueDb;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<ClinicDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ClinicDbContext(
            optionsBuilder.Options,
            new NoOpMediator(),
            new DesignTimeCurrentUserService());
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public string? Email => null;
        public string? Role => null;
        public bool IsAuthenticated => false;
        public bool IsInRole(string role) => false;
    }

    private sealed class NoOpMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default) => Task.FromResult(default(TResponse)!);

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest => Task.CompletedTask;

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

        public Task Publish(
            object notification,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) => EmptyAsync<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default) => EmptyAsync<object?>();

        private static async IAsyncEnumerable<T> EmptyAsync<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
