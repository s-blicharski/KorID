﻿using KorID.Data.Entities;
using KorID.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace KorID.Data;

public class KorIdDbContext : DbContext
{
    public KorIdDbContext(DbContextOptions<KorIdDbContext> options) : base(options)
    {
    }

    public DbSet<Model.User> Users { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserApplication> UserApplications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Npgsql identity defaults are handled by migrations snapshot generator already.

        // Organization
        modelBuilder.Entity<Organization>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Name).IsRequired();
            b.Property(o => o.Slug).IsRequired();
            b.HasIndex(o => o.Slug).IsUnique();
            b.ToTable("Organizations");
        });

        // Application / Client
        modelBuilder.Entity<Application>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Name).IsRequired();
            b.Property(a => a.ClientId).IsRequired();
            b.Property(a => a.ClientSecret).IsRequired();
            b.Property(a => a.RedirectUri).IsRequired();

            b.HasIndex(a => a.ClientId).IsUnique();

            b.HasOne(a => a.Organization)
                .WithMany(o => o.Applications)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            b.ToTable("Applications");
        });

        // Role
        modelBuilder.Entity<Role>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Name).IsRequired();
            b.Property(r => r.ResourceGroup).IsRequired();
            b.ToTable("Roles");
        });

        // User
        modelBuilder.Entity<Model.User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).IsRequired();
            b.Property(u => u.Email).IsRequired();
            b.Property(u => u.PasswordHash).IsRequired();

            b.HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            b.ToTable("Users");
        });

        // UserApplication (many-to-many join)
        modelBuilder.Entity<UserApplication>(b =>
        {
            b.HasKey(ua => new { ua.UserId, ua.ApplicationId });

            b.HasOne(ua => ua.User)
                .WithMany(u => u.UserApplications)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ua => ua.Application)
                .WithMany(a => a.UserApplications)
                .HasForeignKey(ua => ua.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(ua => ua.GrantedAt).IsRequired();
            b.ToTable("UserApplications");
        });
    }
}
