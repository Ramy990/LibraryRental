using LibraryRental.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryRental.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<IRentalMaintenanceService, RentalMaintenanceService>();
        return services;
    }
}
