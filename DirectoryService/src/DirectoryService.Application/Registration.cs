using DirectoryService.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Registration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Registration).Assembly);

        services.Scan(scan => scan
            .FromAssemblies(typeof(Registration).Assembly)
            .AddClasses(classes => classes
                .AssignableToAny(
                    typeof(ICommandHandler<,>),
                    typeof(ICommandHandler<>),
                    typeof(IQueryHandler<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}