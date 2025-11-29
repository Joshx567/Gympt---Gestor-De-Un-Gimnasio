using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class CreateClientModel : PageModel
{
    private readonly ClientApiClient _api;

    public CreateClientModel(ClientApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public ClientDTO Client { get; set; } = new ClientDTO()
    {
        IsActive = true,
        FitnessLevel = "Principiante",
        InitialWeightKg = 0,
        CurrentWeightKg = 0,
        EmergencyContactPhone = "",
        CreatedBy = "System",
        CreatedAt = DateTime.UtcNow
    };

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Asignamos valores por defecto antes de enviar
        Client.CreatedBy = "System";
        Client.CreatedAt = DateTime.UtcNow;

        // Llamada al microservicio para crear el cliente
        var result = await _api.CreateClientAsync(Client);

        if (!result.IsSuccess)
        {
            // Agregamos el error devuelto por el microservicio al ModelState
            ModelState.AddModelError(string.Empty, result.Error);
            return Page();
        }

        // Redirigir a la lista de clientes si todo fue exitoso
        return RedirectToPage("/Clients/Clients");
    }
}
