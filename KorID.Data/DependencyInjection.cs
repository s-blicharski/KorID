using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KorID.Data.Repositories;

namespace KorID.Data;

public static class DependencyInjection
{
    /// <summary>
    /// Registers DbContext and data repositories.
    /// Call from API project's Program.cs:
    ///     builder.Services.AddDataServices(builder.Configuration, "koriddb");
    /// </summary>
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration, string connectionName = "koriddb")
    {
        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        services.AddDbContext<KorIdDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}