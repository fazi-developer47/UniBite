using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

namespace UniversityCanteenFoodOrderingSystem.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        // GET: ProfileController/Details/5
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }



        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var results = await _userManager.UpdateAsync(user);
            if (results.Succeeded)
            {
                TempData["Message"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Details));
            }
            else
            {
                TempData["Error"] = "Error updating profile.";
            }
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _signInManager.SignOutAsync(); // logout first
                await _userManager.DeleteAsync(user); // delete account

            }
            TempData["Message"] = "Your account has been deleted successfully.";
            return RedirectToAction("Index", "Home"); // back to homepage }
        }

    }
}

