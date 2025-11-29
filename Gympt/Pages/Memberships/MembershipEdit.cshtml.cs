using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gympt.Pages.Memberships
{
    public class MembershipEditModel : PageModel
    {
        private readonly MembershipApiClient _service;

        public MembershipEditModel(MembershipApiClient service)
        {
            _service = service;
        }

        [BindProperty]
        public MembershipDTO Membership { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _service.GetMembershipByIdAsync(id);

            if (entity == null)
                return RedirectToPage("Memberships");

            Membership = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _service.UpdateMembershipAsync(Membership.Id, Membership);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            return RedirectToPage("Memberships");
        }
    }
}
