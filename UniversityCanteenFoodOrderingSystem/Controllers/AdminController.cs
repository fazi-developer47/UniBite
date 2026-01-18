using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;
using UniversityCanteenFoodOrderingSystem.Models;

namespace UniversityCanteenFoodOrderingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly DBContextUniBite _db;

        // Constructor injection
        public AdminController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, DBContextUniBite db)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            _db = db;
        }

        // GET: AdminController
        public IActionResult Index()
        {
            // Registered Users
            var totalUsers = _db.Users.Count();

            // Menu Items
            var totalMenuItems = _db.MenuItems.Count();

            // Orders
            var totalOrders = _db.Orders.Count();

            // Monthly Earnings (Delivered orders in current month)
            var monthlyEarnings = _db.Orders
                .Where(o => o.OrderDate.Month == DateTime.Now.Month &&
                            o.OrderDate.Year == DateTime.Now.Year &&
                            o.OrderStatus == OrderStatus.Delivered)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            // Total Earnings (All delivered orders)
            var totalEarnings = _db.Orders
                .Where(o => o.OrderStatus == OrderStatus.Delivered)
                .Sum(o => (decimal?)o.TotalPrice) ?? 0;

            // Feedbacks
            var totalFeedbacks = _db.Feedbacks.Count();
          

            // Task Progress (delivered vs total orders)
            var deliveredOrders = _db.Orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
            var taskProgress = totalOrders > 0 ? (int)((double)deliveredOrders / totalOrders * 100) : 0;

            // Pending Orders (actual count)
            var pendingOrders = _db.Orders.Count(o => o.OrderStatus == OrderStatus.Pending);

            // Monthly Chart Data (12 months earnings)
            var monthlyChartData = Enumerable.Range(1, 12)
                .Select(m => _db.Orders
                    .Where(o => o.OrderDate.Month == m &&
                                o.OrderDate.Year == DateTime.Now.Year &&
                                o.OrderStatus == OrderStatus.Delivered)
                    .Sum(o => (decimal?)o.TotalPrice) ?? 0)
                .ToList();

            // Revenue Sources (only 2 methods in your system)
            var cashOnPickup = _db.Orders.Count(o => o.PaymentMethod == PaymentMethod.CashOnPickup);
            var onlineTransfer = _db.Orders.Count(o => o.PaymentMethod == PaymentMethod.OnlineTransferOnPickup);

            // Recent Orders
            var recentOrders = _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new {
                    o.OrderId,
                    CustomerName = o.UserId, // fallback if navigation missing
                    o.TotalPrice,
                    o.OrderStatus,
                    o.OrderDate,
                    o.PaymentMethod
                })
                .ToList();

            // Pass to View
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalMenuItems = totalMenuItems;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.MonthlyEarnings = monthlyEarnings;
            ViewBag.TotalEarnings = totalEarnings;
            ViewBag.TotalFeedbacks = totalFeedbacks;
            ViewBag.TaskProgress = taskProgress;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.MonthlyChartData = monthlyChartData;
            ViewBag.RevenueSources = new[] { cashOnPickup, onlineTransfer };
            ViewBag.RecentOrders = recentOrders;

            return View();
        }



        // GET: AdminController/Details/5
        public async Task<IActionResult> Details()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminDetails = await _userManager.FindByIdAsync(adminId);

            if (adminDetails == null)
            {
                return NotFound();
            }

            return View(adminDetails);
        }

        // GET: Admin/Edit
        public async Task<IActionResult> Edit()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminDetails = await _userManager.FindByIdAsync(adminId);

            if (adminDetails == null)
            {
                return NotFound();
            }

            return View(adminDetails);
        }

        // POST: Admin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser updatedAdmin)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminDetails = await _userManager.FindByIdAsync(adminId);

            if (adminDetails == null)
            {
                return NotFound();
            }

            //  Always allow FullName update
            adminDetails.FullName = updatedAdmin.FullName;

            //  Detect Email Change
            if (adminDetails.Email != updatedAdmin.Email)
            {
                //  Check if new email is already taken
                var existingUser = await _userManager.FindByEmailAsync(updatedAdmin.Email);
                if (existingUser != null && existingUser.Id != adminDetails.Id)
                {
                    ModelState.AddModelError("Email", "This email is already in use by another account.");
                    return View(updatedAdmin);
                }

                //  Generate token for email change
                var rawToken = await _userManager.GenerateChangeEmailTokenAsync(adminDetails, updatedAdmin.Email);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

                //  Log token for debugging
                Console.WriteLine($"Email Change Token: {encodedToken}");

                //  Redirect to confirmation page
                return RedirectToPage("/Account/RegisterConfirmation", new
                {
                    area = "Identity",
                    email = updatedAdmin.Email,
                    token = encodedToken
                });
            }

            //  If email not changed, update normally
            var result = await _userManager.UpdateAsync(adminDetails);

            if (result.Succeeded)
            {
                TempData["Message"] = "Admin profile updated successfully.";
                return RedirectToAction(nameof(Details));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(updatedAdmin);
        }

        [HttpPost]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            // Try match with User
            var user = _db.Users.FirstOrDefault(u =>
                u.FullName.Contains(query) || u.Email.Contains(query));
            if (user != null)
                return RedirectToAction("Details", "User", new { id = user.Id });

            // Try match with Order
            var order = _db.Orders.FirstOrDefault(o =>
                o.OrderId.ToString() == query || o.UserId.Contains(query));
            if (order != null)
                return RedirectToAction("Details", "Order", new { id = order.OrderId });

            // Try match with Menu Item
            var item = _db.MenuItems.FirstOrDefault(m => m.Name.Contains(query));
            if (item != null)
                return RedirectToAction("Details", "MenuItem", new { id = item.ItemId });

            // Try match with Feedback
            var feedback = _db.Feedbacks.FirstOrDefault(f => f.Comment.Contains(query));
            if (feedback != null)
                return RedirectToAction("Details", "Feedback", new { id = feedback.FeedbackId });

            // If nothing matched, fallback to dashboard
            TempData["Error"] = $"No match found for '{query}'";
            return RedirectToAction("Index");
        }


        // Admin logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear all session values
            HttpContext.Session.Clear();

            // Proper Identity logout
            await _signInManager.SignOutAsync();

            // Redirect to login page 
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }



    }
}
