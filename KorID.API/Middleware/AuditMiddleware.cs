using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KorID.Data;
using KorID.Data.Models;

namespace KorID.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, KorIdDbContext dbContext)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();

            // Przeka¿ ¿±danie dalej w potoku
            await _next(context);

            // Logujemy operacje zmieniaj±ce stan oraz wszystkie zablokowane próby (403)
            if (method == "POST" || method == "PUT" || method == "DELETE" || context.Response.StatusCode == 403)
            {
                var username = context.User?.Identity?.Name ?? "Niezalogowany";
                
                var log = new AuditLog
                {
                    Username = username,
                    Action = $"{method} {path}",
                    Path = path,
                    Method = method,
                    Timestamp = DateTime.UtcNow,
                    StatusCode = context.Response.StatusCode,
                    Details = context.Response.StatusCode == 403 
                        ? $"NARUSZENIE BEZPIECZEÑSTWA: Odmowa dostêpu dla u¿ytkownika {username}" 
                        : $"Pomy¶lna modyfikacja zasobu przez {username}"
                };

                dbContext.AuditLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}