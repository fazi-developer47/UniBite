using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;
using System.Security.Claims;

namespace UniversityCanteenFoodOrderingSystem.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<AppUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public string Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string token, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            AppUser user;

            // ✅ If admin is logged in, load by ID (not email)
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user = await _userManager.FindByIdAsync(userId);
            }
            else
            {
                // ✅ Normal registration flow
                user = await _userManager.FindByEmailAsync(email);
            }

            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            if (!string.IsNullOrEmpty(token))
            {
                // ✅ Token is already encoded
                var encodedToken = token;

                // ✅ Use correct confirmation page based on role
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
                {
                    EmailConfirmationUrl = Url.Page(
                        "/Account/ConfirmEmailChange",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, email = email, code = encodedToken, returnUrl },
                        protocol: Request.Scheme);
                }
                else
                {
                    EmailConfirmationUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = encodedToken, returnUrl },
                        protocol: Request.Scheme);
                }

                DisplayConfirmAccountLink = true;
            }
            else
            {
                DisplayConfirmAccountLink = false;
            }

            return Page();
        }
    }
}
