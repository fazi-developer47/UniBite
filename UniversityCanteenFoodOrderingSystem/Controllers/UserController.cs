using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;

namespace UniversityCanteenFoodOrderingSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser>_userManager;  

        public UserController(UserManager<AppUser> userManager)
        {
            this._userManager = userManager;
        }

        // GET: UserController
        //List all users
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // GET: UserController/Details/5
        //View user details
        public IActionResult Details(string id)
        {
            var userDetails = _userManager.Users.FirstOrDefault( u => u.Id == id);
            if(userDetails == null)
            {
                return NotFound();
            }
            return View(userDetails);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser user, string password)
        {
            if (ModelState.IsValid)
            {
                // Identity requires UserName
                user.UserName = user.Email;
                user.RegisteredOn = DateTime.Now;
                user.EmailConfirmed = true; // Set email as confirmed
                user.PhoneNumberConfirmed = true;// Set phone number as confirmed

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var role = string.IsNullOrEmpty(user.UserType) ? "Customer" : user.UserType;
                    await _userManager.AddToRoleAsync(user, role);

                    TempData["Message"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // Show Identity errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                TempData["ErrorMessage"] = "Error creating user.";
            }

            return View(user);
        }




        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUser updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update custom properties
                user.FullName = updatedUser.FullName;
                user.Email = updatedUser.Email;
                user.UserName = updatedUser.Email; // keep username same as email
                user.UserType = updatedUser.UserType;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Optional: update role if UserType changed
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(user.UserType))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, user.UserType);
                    }
                    TempData["Message"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Error updating user.";
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(updatedUser);
        }


        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // show confirmation page
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = "Error deleting user.";
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

    }
}
