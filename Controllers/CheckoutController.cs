using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spode.Data;
using Spode.Models;
using System.Text.Json;

namespace Spode.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            // Get cart items from session
            var cart = GetCartFromSession();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            // Load product details for cart items
            var cartItems = new List<CartItem>();
            decimal totalPrice = 0;

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Validate stock
                    if (product.StockQuantity < item.Quantity)
                    {
                        TempData["Error"] = $"Sorry, {product.Name} has only {product.StockQuantity} items in stock. Please update your cart.";
                        return RedirectToAction("Index", "Cart");
                    }

                    item.Product = product;
                    cartItems.Add(item);
                    totalPrice += product.Price * item.Quantity;
                }
            }

            // Pass cart items and total price to view
            ViewBag.CartItems = cartItems;
            ViewBag.TotalPrice = totalPrice;

            // Pre-fill checkout form if user is logged in
            var viewModel = new CheckoutViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    viewModel.FullName = user.Name ?? "";
                    viewModel.Email = user.Email ?? "";
                    viewModel.PhoneNumber = user.PhoneNumber ?? "";
                }
            }

            return View(viewModel);
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            // Debug info
            Console.WriteLine($"Received Country value: '{model.Country}'");

            // Ensure Country has a value before validation
            if (string.IsNullOrEmpty(model.Country))
            {
                model.Country = "Egypt";
                Console.WriteLine("Set default country to Egypt");
            }
            
            // Remove any ModelState errors for Country
            if (ModelState.ContainsKey("Country"))
            {
                ModelState.Remove("Country");
            }
            
            if (!ModelState.IsValid)
            {
                // If model validation fails, reload the cart items
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

                ViewBag.CartItems = cartItems;
                ViewBag.TotalPrice = totalPrice;
                return View("Index", model);
            }

            // Get cart items from session
            var sessionCart = GetCartFromSession();
            if (sessionCart == null || !sessionCart.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            // Calculate total price and create OrderItems
            decimal orderTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in sessionCart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Validate stock again
                    if (product.StockQuantity < item.Quantity)
                    {
                        TempData["Error"] = $"Sorry, {product.Name} has only {product.StockQuantity} items in stock. Please update your cart.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Create OrderItem
                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductPrice = product.Price,
                        Quantity = item.Quantity
                    });

                    // Update product stock
                    product.StockQuantity -= item.Quantity;
                    _context.Update(product);

                    // Calculate total
                    orderTotal += product.Price * item.Quantity;
                }
            }

            // Create Order
            var order = new Order
            {
                UserId = User.Identity.IsAuthenticated ? 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : 
                    "guest-" + Guid.NewGuid().ToString(),
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalPrice = orderTotal,
                OrderItems = orderItems
            };

            // Add the shipping address info in OrderNotes for now
            // In a real app, we would extend the Order model to include shipping details
            var shippingDetails = $"Shipping Address:\n{model.FullName}\n{model.AddressLine1}";
            if (!string.IsNullOrEmpty(model.AddressLine2))
                shippingDetails += $"\n{model.AddressLine2}";
            shippingDetails += $"\n{model.City}, {model.State} {model.PostalCode}\n{model.Country}";
            shippingDetails += $"\nPhone: {model.PhoneNumber}\nEmail: {model.Email}";
            shippingDetails += $"\nPayment Method: {model.PaymentMethod}";
            
            if (!string.IsNullOrEmpty(model.OrderNotes))
                shippingDetails += $"\n\nOrder Notes: {model.OrderNotes}";

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear cart after successful order
            HttpContext.Session.Remove("Cart");

            // Store order ID in TempData for confirmation page
            TempData["OrderId"] = order.Id;
            TempData["Success"] = "Your order has been placed successfully!";

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        // GET: Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            // Only allow access to order confirmation if it matches the TempData["OrderId"]
            if (!TempData.ContainsKey("OrderId") || (int)TempData["OrderId"] != id)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Helper method to get cart from session - same as in CartController
        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }
    }
} 