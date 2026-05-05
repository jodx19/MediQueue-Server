// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\DependencyInjection.cs
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MediQueue.Application.Common.Behaviors;

namespace MediQueue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        // Register MediatR and Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Register behaviors in correct order
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation Validators
        // Assuming FluentValidation.DependencyInjectionExtensions is installed
        services.AddValidatorsFromAssembly(assembly);

        // Register Memory Cache (required for ICacheService implementations that use it)
        services.AddMemoryCache();

        return services;
    }
}
