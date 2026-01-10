using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KorID.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedExternalLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert demo organization
            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[] { 100, "Demo Organization", "demo-org" });

            // Insert demo application linked to organization
            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "Name", "ClientId", "ClientSecret", "RedirectUri", "OrganizationId" },
                values: new object[] { 200, "Demo App", "demo-client-id", "demo-client-secret", "https://example.com/callback", 100 });

            // Insert demo user in the same organization
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "Email", "PasswordHash", "OrganizationId" },
                values: new object[] { 300, "demo.user", "demo@example.com", "$DEMO$HASH$changeme", 100 });

            // Grant demo user access to demo application
            migrationBuilder.InsertData(
                table: "UserApplications",
                columns: new[] { "UserId", "ApplicationId", "GrantedAt" },
                values: new object[] { 300, 200, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the grant
            migrationBuilder.DeleteData(
                table: "UserApplications",
                keyColumns: new[] { "UserId", "ApplicationId" },
                keyValues: new object[] { 300, 200 });

            // Remove user
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 300);

            // Remove application
            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 200);

            // Remove organization
            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 100);
        }
    }
}
