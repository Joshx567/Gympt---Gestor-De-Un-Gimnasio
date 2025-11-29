using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class EditClientModel : PageModel
{
    private readonly ClientApiClient _api;

    public EditClientModel(ClientApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public ClientDTO Client { get; set; } = new ClientDTO()
    {
        LastModifiedBy = "System",
        LastModification = DateTime.UtcNow,
    };

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Client = await _api.GetClientByIdAsync(id);
        if (Client == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Valores de modificación por defecto
        Client.CreatedBy ??= "System";
        Client.CreatedAt = DateTime.UtcNow;

        Client.LastModifiedBy ??= "System";
        Client.LastModification = DateTime.UtcNow;

        var result = await _api.UpdateClientAsync(Client.Id,Client);
        if (!result.IsSuccess)
        {
            // Mostramos errores devueltos por el microservicio
            ModelState.AddModelError(string.Empty, result.Error);
            return Page();
        }

        return RedirectToPage("/Clients/Clients");
    }

}
