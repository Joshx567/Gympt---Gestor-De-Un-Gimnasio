using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ClientsModel : PageModel
{
    private readonly ClientApiClient _api;

    public ClientsModel(ClientApiClient api)
    {
        _api = api;
    }

    public List<ClientDTO> Clients { get; set; } = new();

    public async Task OnGetAsync()
    {
        Clients = await _api.GetClientsAsync();
    }

    // Handler para eliminar
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        bool deleted = await _api.DeleteClientAsync(id);

        if (!deleted)
        {
            TempData["Error"] = "No se pudo eliminar el cliente.";
        }
            
        // Recargar la página
        return RedirectToPage();
    }
}
