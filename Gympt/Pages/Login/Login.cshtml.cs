using Gympt.Application.Facades;
using Gympt.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Gympt.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly GymFacade _gymFacade;

        [BindProperty]
        public LoginRequestDTO Login { get; set; } = new LoginRequestDTO();

        public LoginModel(GymFacade gymFacade)
        {
            _gymFacade = gymFacade;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var loginResult = await _gymFacade.LoginUserAsync(Login);

                if (loginResult == null || loginResult.user == null || string.IsNullOrEmpty(loginResult.token))
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    return Page();
                }

                // 🔹 Guardar token explícitamente en Session
                HttpContext.Session.SetString("jwt_token", loginResult.token);
                Console.WriteLine("Session Id: " + HttpContext.Session.Id);
                Console.WriteLine("Token desde Session: " + HttpContext.Session.GetString("jwt_token"));

                // 🔹 Guardar Claims para la cookie (también incluimos el token como claim)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResult.user.Id.ToString()),
                    new Claim(ClaimTypes.Name, loginResult.user.Name),
                    new Claim(ClaimTypes.Email, loginResult.user.Email),
                    new Claim(ClaimTypes.Role, loginResult.user.Role),
                    new Claim("must_change_password", loginResult.user.MustChangePassword.ToString()),
                    new Claim("access_token", loginResult.token) // 🔹 token incluido como claim
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties
                );

                // Redirigir si debe cambiar la contraseña
                if (loginResult.user.MustChangePassword)
                    return RedirectToPage("/Login/ChangePassword");

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio de autenticación.");
                Console.WriteLine(ex);
                return Page();
            }
        }
    }
}
