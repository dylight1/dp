using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spode.Data;
using Spode.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spode.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string status = null, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            // Get total count for pagination
            var totalOrders = await query.CountAsync();
            
            // Get paginated results
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.StatusList = new List<string> { "All", "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }

            // Update order status
            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Order #{id} status updated to {status}!";
            
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 