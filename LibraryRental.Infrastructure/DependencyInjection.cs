using LibraryRental.Application.Common.Interfaces;
using LibraryRental.Infrastructure.Identity;
using LibraryRental.Infrastructure.Persistence;
using LibraryRental.Infrastructure.Repositories;
using LibraryRental.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryRental.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILibrarianRepository, LibrarianRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IBookCopyRepository, BookCopyRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();

        // Hangfire resolves this through DI when it runs the job
        services.AddScoped<INotificationService, EmailNotificationService>();

        return services;
    }
}
