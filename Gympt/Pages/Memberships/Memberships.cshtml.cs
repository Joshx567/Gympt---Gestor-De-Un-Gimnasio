using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Gympt.Pages.Memberships
{
    public class MembershipsModel : PageModel
    {
        private readonly MembershipApiClient _service;

        public MembershipsModel(MembershipApiClient service)
        {
            _service = service;
        }

        public List<MembershipDTO> Memberships { get; set; } = new();

        public async Task OnGetAsync()
        {
            Memberships = await _service.GetMembershipsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _service.DeleteMembershipAsync(id);
            return RedirectToPage();
        }
    }
}
