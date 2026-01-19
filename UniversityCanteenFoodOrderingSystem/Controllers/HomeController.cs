using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;
using UniversityCanteenFoodOrderingSystem.Models;
using UniversityCanteenFoodOrderingSystem.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace UniversityCanteenFoodOrderingSystem.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBContextUniBite _db;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, DBContextUniBite db, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        // Show home page with menu items and active order status
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            //  Redirect admin users to Admin dashboard
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var userId = _userManager.GetUserId(User);

            bool hasActiveOrder = false;

            if (!string.IsNullOrEmpty(userId))
            {
                //  Only check orders if user is logged in
                hasActiveOrder = _db.Orders.Any(o => o.UserId == userId &&
                    (o.OrderStatus == OrderStatus.Pending ||
                     o.OrderStatus == OrderStatus.Preparing ||
                     o.OrderStatus == OrderStatus.Ready));
            }

            //  Get all menu items
            var menuItems = _db.MenuItems.ToList();

            //  Pass active order flag to view (always true/false, never null)
            ViewBag.HasActiveOrder = hasActiveOrder;

            return View(menuItems);
        }


        // Show privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Submit feedback for menu items
        [HttpPost]
        public IActionResult SubmitFeedback(Feedback feedback)
        {
            // save feedback to database
            _db.Feedbacks.Add(feedback);
            _db.SaveChanges();

            // show success message
            TempData["Message"] = "Thanks for your feedback!";

            // redirect back to home page
            return RedirectToAction("Index");
        }

        // Add item to cart
        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity = 1)
        {
            // find menu item
            var menuItem = _db.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
            if (menuItem == null) return RedirectToAction("Index");

            // get cart from session or create new
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // check if item already exists in cart
            var existingItem = cart.FirstOrDefault(c => c.ItemId == itemId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity; // increase quantity
            }
            else
            {
                // add new item to cart
                cart.Add(new CartItem
                {
                    ItemId = menuItem.ItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = quantity
                });
            }

            // save cart back to session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // show success message
            TempData["Message"] = $"{menuItem.Name} (x{quantity}) added to cart!";
            return RedirectToAction("Index");
        }

        // Show cart page
        public IActionResult Cart()
        {
            // get cart items from session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // Remove item from cart
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            // get cart from session using key "Cart"
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (cart != null)
            {
                // find item to remove
                var itemToRemove = cart.FirstOrDefault(c => c.ItemId == itemId);
                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);

                    // update cart in session
                    HttpContext.Session.SetObjectAsJson("Cart", cart);

                    TempData["Message"] = "Item removed from cart.";
                }
                else
                {
                    TempData["Error"] = "Item not found in cart.";
                }
            }
            else
            {
                TempData["Error"] = "Cart is empty.";
            }

            return RedirectToAction("Cart");
        }


        // Confirm order from cart
        [HttpPost]
        public IActionResult ConfirmOrder(string paymentMethod)
        {
            var userId = _userManager.GetUserId(User);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            //  Check if cart is empty
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }

            //  Validate payment method
            if (string.IsNullOrEmpty(paymentMethod) || !Enum.TryParse<PaymentMethod>(paymentMethod, true, out var parsedMethod))
            {
                TempData["Error"] = "Please select a valid payment method.";
                return RedirectToAction("Cart");
            }

            //  Check for existing pending order
            var existingOrder = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.UserId == userId && o.OrderStatus == OrderStatus.Pending);

            if (existingOrder != null)
            {
                //  Add items to existing order
                foreach (var c in cart)
                {
                    existingOrder.OrderDetails.Add(new OrderDetail
                    {
                        MenuItemId = c.ItemId,
                        Quantity = c.Quantity,
                        Price = c.Price
                    });
                }

                //  Update total price
                existingOrder.TotalPrice = existingOrder.OrderDetails.Sum(d => d.Quantity * d.Price);
                _db.SaveChanges();

                TempData["Message"] = $"Order updated! ID: {existingOrder.OrderId}, Total: Rs. {existingOrder.TotalPrice}";
            }
            else
            {
                //  Create new order
                var newOrder = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    PaymentMethod = parsedMethod,
                    OrderStatus = OrderStatus.Pending,
                    OrderDetails = cart.Select(c => new OrderDetail
                    {
                        MenuItemId = c.ItemId,
                        Quantity = c.Quantity,
                        Price = c.Price
                    }).ToList()
                };

                newOrder.TotalPrice = newOrder.OrderDetails.Sum(d => d.Quantity * d.Price);

                _db.Orders.Add(newOrder);
                _db.SaveChanges();

                TempData["Message"] = $"Order placed! ID: {newOrder.OrderId}, Total: Rs. {newOrder.TotalPrice}";
            }

            //  Clear cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Orders");
        }


        // Place direct order for single item
        [HttpPost]
        public IActionResult DirectOrder(int itemId, int quantity = 1, string paymentMethod = "CashOnPickup")
        {
            var userId = _userManager.GetUserId(User);
            var menuItem = _db.MenuItems.FirstOrDefault(m => m.ItemId == itemId);

            if (menuItem == null)
            {
                TempData["Error"] = "Invalid menu item!";
                return RedirectToAction("Index");
            }

            //  Validate payment method safely
            if (string.IsNullOrEmpty(paymentMethod) || !Enum.TryParse<PaymentMethod>(paymentMethod, true, out var parsedMethod))
            {
                TempData["Error"] = "Please select a valid payment method.";
                return RedirectToAction("Index");
            }

            //  Check if user has a pending order
            var existingOrder = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.UserId == userId && o.OrderStatus == OrderStatus.Pending);

            if (existingOrder != null)
            {
                //  Add item to existing order
                existingOrder.OrderDetails.Add(new OrderDetail
                {
                    MenuItemId = menuItem.ItemId,
                    Quantity = quantity,
                    Price = menuItem.Price
                });

                //  Update total price
                existingOrder.TotalPrice = existingOrder.OrderDetails.Sum(d => d.Quantity * d.Price);
                _db.SaveChanges();

                TempData["Message"] = $"Item added! Order ID: {existingOrder.OrderId}, Total: Rs. {existingOrder.TotalPrice}";
            }
            else
            {
                //  Create new order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    PaymentMethod = parsedMethod,
                    OrderStatus = OrderStatus.Pending,
                    OrderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    MenuItemId = menuItem.ItemId,
                    Quantity = quantity,
                    Price = menuItem.Price
                }
            }
                };

                //  Calculate total price
                order.TotalPrice = order.OrderDetails.Sum(d => d.Quantity * d.Price);

                _db.Orders.Add(order);
                _db.SaveChanges();

                TempData["Message"] = $"Direct order placed! ID: {order.OrderId}, Total: Rs. {order.TotalPrice}";
            }

            return RedirectToAction("Orders");
        }


        // Show all non-deleted orders of logged-in user
        public IActionResult Orders()
        {
            var userId = _userManager.GetUserId(User);

            // get all orders with details, skip deleted ones
            var orders = _db.Orders
                .Where(o => o.UserId == userId && !o.IsDeletedByUser) // filter added
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.MenuItem)
                .ToList();

            return View(orders);
        }


        // Update payment method of an order
        [HttpPost]
        public IActionResult UpdateOrderPayment(int orderId, string paymentMethod)
        {
            var userId = _userManager.GetUserId(User);

            // find order by ID and user
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (order != null)
            {
                // try to parse payment method
                if (Enum.TryParse<PaymentMethod>(paymentMethod, out var parsedMethod))
                {
                    order.PaymentMethod = parsedMethod; // update payment method
                    _db.SaveChanges();
                    TempData["Message"] = "Payment method updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Invalid payment method.";
                }
            }
            else
            {
                TempData["Error"] = "Order not found.";
            }

            return RedirectToAction("Orders");
        }


        //Soft Orders delete from user side
        [HttpPost]
        public IActionResult RemoveCompletedOrders()
        {
            var userId = _userManager.GetUserId(User);

            var completedOrders = _db.Orders
                .Where(o => o.UserId == userId && o.OrderStatus == OrderStatus.Delivered && !o.IsDeletedByUser)
                .ToList();

            foreach (var order in completedOrders)
            {
                order.IsDeletedByUser = true;
            }

            _db.SaveChanges();
            TempData["Message"] = "Your completed orders have been removed from your dashboard.";

            return RedirectToAction("Orders");
        }


        // Download receipt for a specific order
        public IActionResult DownloadReceipt(int orderId)
        {
            var userId = _userManager.GetUserId(User);

            // find order with details
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.MenuItem)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            // return receipt view
            return View("DownloadReceipt", order);
        }

        // Error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
