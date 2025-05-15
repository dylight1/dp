using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spode.Data;

namespace Spode.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Dashboard stats
            ViewBag.ProductCount = _context.Products.Count();
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.UserCount = _context.Users.Count();
            
            return View();
        }
    }
} 