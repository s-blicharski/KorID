using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace KorID.Data;

public sealed class KorIdDbContextFactory : IDesignTimeDbContextFactory<KorIdDbContext>
{
    private const string AspireEnvConnectionString = "ConnectionStrings__koriddb";

    public KorIdDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable(AspireEnvConnectionString);

        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                $"Brak connection stringa. Ustaw zmienn¹ œrodowiskow¹ {AspireEnvConnectionString} przed wywo³aniem 'dotnet ef', " +
                "np.\nWindows (PowerShell): $env:ConnectionStrings__koriddb = \"Host=localhost;Port=5432;Database=koriddb;Username=postgres;Password=postgres\";\n" +
                "Linux/macOS (bash): export ConnectionStrings__koriddb=\"Host=localhost;Port=5432;Database=koriddb;Username=postgres;Password=postgres\"");
        }

        var options = new DbContextOptionsBuilder<KorIdDbContext>()
            .UseNpgsql(conn)
            .EnableSensitiveDataLogging() // pomocne przy lokalnym generowaniu migracji; usuñ w prod jeœli niepotrzebne
            .Options;

        return new KorIdDbContext(options);
    }
}