using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GYMPT.Pages.Users
{
    public class UserEditModel : PageModel
    {
        private readonly UserApiClient _userApi;

        [BindProperty]
        public UserDTO Instructor { get; set; } = new UserDTO();

        public UserEditModel(UserApiClient userApi)
        {
            _userApi = userApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            // **CONFIGURAR TOKEN**
            _userApi.SetJwtToken(token);

            try
            {
                Instructor = await _userApi.GetByIdAsync(id);
                if (Instructor == null) return NotFound();
            }
            catch
            {
                return RedirectToPage("/Users/Users");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var token = HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            // **CONFIGURAR TOKEN**
            _userApi.SetJwtToken(token);

            try
            {
                await _userApi.UpdateUserAsync(Instructor.id, Instructor);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            TempData["SuccessMessage"] = $"Usuario '{Instructor.name} {Instructor.first_lastname}' actualizado correctamente.";
            return RedirectToPage("/Users/Users");
        }
    }
}
