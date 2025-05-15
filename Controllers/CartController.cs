using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spode.Data;
using Spode.Models;
using System.Text.Json;

namespace Spode.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cart = GetCartFromSession();
            var cartItems = new List<CartItem>();
            decimal totalPrice = 0;

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    item.Product = product;
                    cartItems.Add(item);
                    totalPrice += product.Price * item.Quantity;
                }
            }

            ViewBag.TotalPrice = totalPrice;
            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Return 401 Unauthorized status with a special code 
                // that our JavaScript can recognize
                return Json(new { success = false, requiresAuth = true });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Validate stock
            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // If product already in cart, update quantity
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                cart.Add(new CartItem 
                { 
                    ProductId = productId, 
                    Quantity = quantity,
                    UserId = GetUserId() 
                });
            }

            SaveCartToSession(cart);
            
            // If it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            // For non-AJAX requests
            TempData["Success"] = $"{product.Name} has been added to your cart!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(productId);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Validate stock
            if (product.StockQuantity < quantity)
            {
                TempData["Error"] = "Not enough stock available.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                SaveCartToSession(cart);
                TempData["Success"] = "Cart has been updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
                TempData["Success"] = "Item has been removed from your cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/ClearCart
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Your cart has been cleared.";
            return RedirectToAction(nameof(Index));
        }

        // Helper methods for cart session management
        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        private string GetUserId()
        {
            // If user is authenticated, use their ID, otherwise use a session ID
            return User.Identity.IsAuthenticated 
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                : HttpContext.Session.Id;
        }

        // Cart Summary (for the cart icon in navbar)
        [HttpGet]
        public async Task<IActionResult> CartSummary()
        {
            var cart = GetCartFromSession();
            int itemCount = cart.Sum(i => i.Quantity);
            decimal total = 0;

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    total += product.Price * item.Quantity;
                }
            }

            return Json(new { itemCount, total });
        }
    }
} 