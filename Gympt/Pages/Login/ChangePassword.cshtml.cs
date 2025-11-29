using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Gympt.Pages.Login
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
    }

    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly HttpClient _userHttp;

        [BindProperty]
        public ChangePasswordDTO Input { get; set; }

        public ChangePasswordModel(IHttpClientFactory factory)
        {
            _userHttp = factory.CreateClient("Users");
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Account/Login");

            var request = new ChangePasswordRequest { NewPassword = Input.NewPassword };
            var response = await _userHttp.PostAsJsonAsync($"api/user/password-update/{userId}", request);

            if (response.IsSuccessStatusCode)
            {
                await HttpContext.SignOutAsync();
                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente. Inicia sesión nuevamente.";
                return RedirectToPage("/Account/Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error al cambiar la contraseña: {error}");
            return Page();
        }
    }
}
