using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Infrastructure.Persistence;
using JewelryFactory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryFactory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // EF Core — Azure SQL Database
        var connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' missing.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connStr, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Auth services
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
