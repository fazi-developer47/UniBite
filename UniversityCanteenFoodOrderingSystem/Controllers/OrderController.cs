using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityCanteenFoodOrderingSystem.Areas.Identity.Data;
using UniversityCanteenFoodOrderingSystem.Models;

namespace UniversityCanteenFoodOrderingSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly DBContextUniBite _context;

        public OrderController(DBContextUniBite context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var dBContextUniBite = _context.Orders.Include(o => o.User);
            return View(await dBContextUniBite.ToListAsync());
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)           // include order details
                    .ThenInclude(od => od.MenuItem)     //include menu item for names
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus orderStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            order.OrderStatus = orderStatus;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order status updated successfully.";
            return RedirectToAction(nameof(Index));
        }



        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
