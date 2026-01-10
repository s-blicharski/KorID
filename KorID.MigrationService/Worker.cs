using KorID.Data;
using KorID.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace KorID.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KorIdDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(KorIdDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }
    private static async Task SeedDataAsync(KorIdDbContext dbContext, CancellationToken cancellationToken)
    {
        // Use execution strategy to handle transient errors when running migrations/seeds on cloud providers
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Ensure admin user exists (legacy seed)
            if (!await dbContext.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "ad@gmail.com",
                    PasswordHash = "change_me_later"
                };

                await dbContext.Users.AddAsync(admin, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Demo organization
            var demoOrg = await dbContext.Organizations.SingleOrDefaultAsync(o => o.Slug == "demo-org", cancellationToken);
            if (demoOrg == null)
            {
                demoOrg = new Organization
                {
                    Name = "Demo Organization",
                    Slug = "demo-org"
                };
                await dbContext.Organizations.AddAsync(demoOrg, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Demo application
            var demoApp = await dbContext.Applications.SingleOrDefaultAsync(a => a.ClientId == "demo-client-id", cancellationToken);
            if (demoApp == null)
            {
                demoApp = new Application
                {
                    Name = "Demo App",
                    ClientId = "demo-client-id",
                    ClientSecret = "demo-client-secret",
                    RedirectUri = "http://localhost:5173/test/callback",
                    OrganizationId = demoOrg.Id
                };
                await dbContext.Applications.AddAsync(demoApp, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Demo user
            var demoUser = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == "demo@example.com" && u.OrganizationId == demoOrg.Id, cancellationToken);
            if (demoUser == null)
            {
                demoUser = new User
                {
                    Username = "demo.user",
                    Email = "demo@example.com",
                    PasswordHash = "$DEMO$HASH$changeme",
                    OrganizationId = demoOrg.Id
                };
                await dbContext.Users.AddAsync(demoUser, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Grant demo user access to demo application
            var hasGrant = await dbContext.UserApplications.AnyAsync(ua => ua.UserId == demoUser.Id && ua.ApplicationId == demoApp.Id, cancellationToken);
            if (!hasGrant)
            {
                var grant = new UserApplication
                {
                    UserId = demoUser.Id,
                    ApplicationId = demoApp.Id,
                    GrantedAt = DateTime.UtcNow
                };
                await dbContext.UserApplications.AddAsync(grant, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
