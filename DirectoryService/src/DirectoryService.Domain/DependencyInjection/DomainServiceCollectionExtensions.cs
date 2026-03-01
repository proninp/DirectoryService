using DirectoryService.Domain.Services;
using DirectoryService.Domain.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Domain.DependencyInjection;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentHierarchyService, DepartmentHierarchyService>();
        return services;
    }
}