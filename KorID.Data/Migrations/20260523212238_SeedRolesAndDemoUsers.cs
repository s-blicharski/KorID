using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KorID.Data.Migrations
{
    public partial class SeedRolesAndDemoUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        -- Role (tabela samodzielna, zero FK)
        INSERT INTO ""Roles"" (""Id"", ""Name"", ""ResourceGroup"") VALUES
            (1, 'admin',  'korid'),
            (2, 'viewer', 'korid')
        ON CONFLICT DO NOTHING;

        -- Demo userzy BEZ organizacji -> OrganizationId NULL = brak zależności FK
        INSERT INTO ""Users"" (""Id"", ""Username"", ""Email"", ""PasswordHash"", ""OrganizationId"") VALUES
            (301, 'admin.demo',  'admin@korid.local',  '$DEMO$HASH$admin123',  NULL),
            (302, 'viewer.demo', 'viewer@korid.local', '$DEMO$HASH$viewer123', NULL)
        ON CONFLICT DO NOTHING;

        -- Przypisanie ról (FK do Users i Roles z tej samej transakcji -> OK)
        INSERT INTO ""UserRoles"" (""UserId"", ""RoleId"") VALUES
            (301, 1),
            (302, 2)
        ON CONFLICT DO NOTHING;
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DELETE FROM ""UserRoles"" WHERE ""UserId"" IN (301, 302);
        DELETE FROM ""Users""     WHERE ""Id""     IN (301, 302);
        DELETE FROM ""Roles""     WHERE ""Id""     IN (1, 2);
    ");
        }
    }
}