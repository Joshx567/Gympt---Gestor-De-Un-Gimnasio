using Gympt.DTO;
using Gympt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Gympt.Common;

namespace Gympt.Pages.Memberships
{
    public class MembershipCreateModel : PageModel
    {
        private readonly MembershipApiClient _service;

        public MembershipCreateModel(MembershipApiClient service)
        {
            _service = service;
        }

        [BindProperty]
        public MembershipDTO Membership { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _service.CreateMembershipAsync(Membership);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            return RedirectToPage("Memberships");
        }
    }
}
