using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Decorators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DirectoryService.Application;

public static class Registration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
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

        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>));

        return services;
    }
}