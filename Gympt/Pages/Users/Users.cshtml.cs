using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Gympt.DTO;
using Gympt.Services;

namespace GYMPT.Pages.Users
{
    public class UsersModel : PageModel
    {
        private readonly UserApiClient _userApiClient;
        public IEnumerable<UserDTO> UserList { get; private set; } = new List<UserDTO>();

        public UsersModel(UserApiClient userApiClient)
        {
            _userApiClient = userApiClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Leer token desde sesión
            var token = HttpContext.Session.GetString("jwt_token");
            Console.WriteLine("=== DEBUG OnGetAsync ===");
            Console.WriteLine("Session Id: " + HttpContext.Session.Id);
            Console.WriteLine("Token desde Session: " + (token ?? "[NO EXISTE]"));

            if (string.IsNullOrEmpty(token))
            {
                // ⚠️ Redirigir al login si no hay token
                return RedirectToPage("/Login/Login");
            }

            // Configurar token en el cliente
            _userApiClient.SetJwtToken(token);

            // Decodificar JWT localmente para ver claims
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                Console.WriteLine("=== CLAIMS dentro del JWT ===");
                foreach (var c in jwt.Claims)
                    Console.WriteLine($"{c.Type} : {c.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al decodificar JWT: " + ex.Message);
            }

            try
            {
                // Llamar al microservicio para obtener usuarios
                UserList = await _userApiClient.GetAllUsersAsync();

                // Log de debug (opcional)
                Console.WriteLine("=== Usuarios recibidos ===");
                foreach (var u in UserList)
                    Console.WriteLine($"{u.id} - {u.name} {u.first_lastname} {u.second_lastname} - {u.email} - {u.role}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al obtener usuarios: " + ex.Message);
                TempData["ErrorMessage"] = "No se pudieron cargar los usuarios.";
            }

            return Page();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Leer token desde sesión
            var token = HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login/Login");

            // Configurar token en UserApiClient
            _userApiClient.SetJwtToken(token);

            try
            {
                await _userApiClient.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
            }
            catch (ApiException ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar usuario: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToPage();
        }

    }
}
