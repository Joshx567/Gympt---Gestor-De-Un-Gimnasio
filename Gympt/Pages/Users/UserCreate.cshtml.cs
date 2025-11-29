using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace Gympt.Pages.Users
{
    public class UserCreateModel : PageModel
    {
        private readonly UserApiClient _userApiClient;

        [BindProperty]
        public UserDTO User { get; set; } = new UserDTO();

        public string TempPassword { get; set; } = string.Empty;

        public UserCreateModel(UserApiClient userApiClient)
        {
            _userApiClient = userApiClient;
        }

        public void OnGet()
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                // **CONFIGURAR TOKEN**
                _userApiClient.SetJwtToken(token);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            // **CONFIGURAR TOKEN**
            _userApiClient.SetJwtToken(token);

            // Validaciones
            if (User.role == "Instructor")
            {
                if (!User.hire_date.HasValue)
                    ModelState.AddModelError("User.hire_date", "La fecha de contratación es requerida.");

                if (!User.monthly_salary.HasValue || User.monthly_salary <= 0)
                    ModelState.AddModelError("User.monthly_salary", "El salario debe ser mayor a cero.");
            }

            if (!ModelState.IsValid) return Page();

            try
            {
                TempPassword = GenerateTemporaryPassword(User.ci);
                User.password = TempPassword;
                User.must_change_password = true;

                await _userApiClient.CreateUserAsync(User);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            TempData["SuccessMessage"] = $"Usuario '{User.name} {User.first_lastname}' creado exitosamente.";
            TempData["TempPassword"] = TempPassword;

            return RedirectToPage("/Users/Users");
        }

        private string GenerateTemporaryPassword(string ci)
        {
            return $"G{ci}t!";
        }
    }
}
