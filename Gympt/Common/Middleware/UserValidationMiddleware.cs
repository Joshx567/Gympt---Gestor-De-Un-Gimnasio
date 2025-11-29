using Gympt.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Gympt.Common.Middleware
{
    public class UserValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserValidationMiddleware> _logger;
        private readonly UserApiClient _userApi;
        private readonly IDataProtector _protector;

        // Rutas públicas que no requieren token
        private static readonly string[] PublicRoutes = new[]
        {
            "/", "/home", "/home/index", "/privacy",
            "/login", "/login/login", "/logout", "/error"
        };

        public UserValidationMiddleware(
            RequestDelegate next,
            ILogger<UserValidationMiddleware> logger,
            UserApiClient userApi,
            IDataProtectionProvider provider)
        {
            _next = next;
            _logger = logger;
            _userApi = userApi;
            _protector = provider.CreateProtector(
                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                "Cookies",
                "v2");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Permitir rutas públicas sin validación
            if (PublicRoutes.Any(r => path.StartsWith(r)))
            {
                await _next(context);
                return;
            }

            Console.WriteLine("=== Middleware UserValidation ===");
            Console.WriteLine($"Session Id: {context.Session.Id}");

            // Leer token desde sesión
            var token = context.Session.GetString("jwt_token");

            if (string.IsNullOrEmpty(token))
            {
                // Intentar obtener token desde cookie
                if (context.Request.Cookies.TryGetValue("jwt_token", out var cookieToken))
                {
                    token = cookieToken;
                    Console.WriteLine("Token recuperado desde cookie.");
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Acceso denegado: no hay token en sesión ni cookie");
                context.Response.Redirect("/Login");
                return;
            }

            // Configurar token en UserApiClient
            _userApi.SetJwtToken(token);

            await _next(context);
        }

        // Método opcional para deserializar cookie si necesitas usarlo
        private string? TryGetTokenFromCookie(string cookieValue, out UserDto? user)
        {
            user = null;

            try
            {
                var ticketDataFormat = new TicketDataFormat(_protector);
                var ticket = ticketDataFormat.Unprotect(cookieValue);

                if (ticket == null)
                    return null;

                var principal = ticket.Principal;

                var token = principal.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

                user = new UserDto
                {
                    Id = int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0,
                    Name = principal.Identity?.Name ?? "",
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    Role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "",
                    MustChangePassword = principal.Claims.Any(c => c.Type == "must_change_password" && c.Value == "true")
                };

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se pudo procesar la cookie: {ex.Message}");
                return null;
            }
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool MustChangePassword { get; set; }
    }
}