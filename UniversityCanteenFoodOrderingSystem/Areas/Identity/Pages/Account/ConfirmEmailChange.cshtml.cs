using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

namespace UniversityCanteenFoodOrderingSystem.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ConfirmEmailChangeModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            // ✅ Parameter validation
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                StatusMessage = "Error: Missing parameters for email confirmation.";
                return Page();
            }

            // ✅ Load user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = $"Error: Unable to load user with ID '{userId}'.";
                return Page();
            }

            // ✅ Decode token safely
            string decodedCode;
            try
            {
                decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch
            {
                StatusMessage = "Error: Invalid confirmation code format.";
                return Page();
            }

            // ✅ Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                StatusMessage = "Error: This email is already in use by another account.";
                return Page();
            }

            // ✅ Step 1: Change Email
            var changeEmailResult = await _userManager.ChangeEmailAsync(user, email, decodedCode);
            if (!changeEmailResult.Succeeded)
            {
                var errors = string.Join(", ", changeEmailResult.Errors.Select(e => e.Description));
                StatusMessage = $"Error changing email: {errors}";
                return Page();
            }

            // ✅ Step 2: Sync Username with Email
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                var errors = string.Join(", ", setUserNameResult.Errors.Select(e => e.Description));
                StatusMessage = $"Error changing username: {errors}";
                return Page();
            }

            // ✅ Step 3: Refresh Sign-in
            await _signInManager.RefreshSignInAsync(user);

            // ✅ Step 4: Role-based redirect
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Message"] = "Admin email updated and confirmed successfully.";
                return RedirectToAction("Details", "Admin");
            }

            StatusMessage = "Your email has been updated and confirmed successfully.";
            return Page();
        }
    }
}
