using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spode.Models;

namespace Spode.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Make sure the database is created
            context.Database.EnsureCreated();

            // Look for roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                // Create Admin role
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                // Create Customer role
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Look for an admin user
            if (!await context.Users.AnyAsync(u => u.Email == "admin@Spode.com"))
            {
                // Create the admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@mail.com",
                    Email = "admin@mail.com",
                    EmailConfirmed = true,
                    Name = "Admin User",
                    ProfilePicture = "/images/no-user-image.png"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create a normal user
            ApplicationUser normalUser = null;
            if (!await context.Users.AnyAsync(u => u.Email == "user@mail.com"))
            {
                normalUser = new ApplicationUser
                {
                    UserName = "user@mail.com",
                    Email = "user@mail.com",
                    EmailConfirmed = true,
                    Name = "John Doe",
                    ProfilePicture = "/images/no-user-image.png"
                };

                var result = await userManager.CreateAsync(normalUser, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "Customer");
                }
            }
            else
            {
                normalUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "user@omarshop.com");
            }

            // Look for products
            if (!await context.Products.AnyAsync())
            {
                // Add sample products
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Smartphone X",
                        Description = "Latest smartphone with advanced features and high-resolution camera.",
                        Price = 799.99m,
                        StockQuantity = 50,
                        ImageUrl = "/images/no-product-image.png"
                    },
                    new Product
                    {
                        Name = "Laptop Pro",
                        Description = "Powerful laptop for professionals with high performance and long battery life.",
                        Price = 1299.99m,
                        StockQuantity = 30,
                        ImageUrl = "/images/no-product-image.png"
                    },
                    new Product
                    {
                        Name = "Wireless Headphones",
                        Description = "Premium wireless headphones with noise cancellation and superior sound quality.",
                        Price = 199.99m,
                        StockQuantity = 100,
                        ImageUrl = "/images/no-product-image.png"
                    },
                    new Product
                    {
                        Name = "Smart Watch",
                        Description = "Feature-rich smartwatch with health tracking and notification capabilities.",
                        Price = 249.99m,
                        StockQuantity = 45,
                        ImageUrl = "/images/no-product-image.png"
                    },
                    new Product
                    {
                        Name = "Bluetooth Speaker",
                        Description = "Portable Bluetooth speaker with superior sound quality and long battery life.",
                        Price = 89.99m,
                        StockQuantity = 75,
                        ImageUrl = "/images/no-product-image.png"
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // Add sample orders if none exist
            if (!await context.Orders.AnyAsync() && normalUser != null)
            {
                // Get products from the database
                var products = await context.Products.ToListAsync();
                if (products.Count > 0)
                {
                    // Create sample orders with different statuses
                    var orders = new List<Order>
                    {
                        new Order
                        {
                            UserId = normalUser.Id,
                            OrderDate = DateTime.Now.AddDays(-10),
                            Status = "Delivered",
                            TotalPrice = products[0].Price + products[2].Price,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem
                                {
                                    ProductId = products[0].Id,
                                    ProductName = products[0].Name,
                                    ProductPrice = products[0].Price,
                                    Quantity = 1,
                                    Product = products[0]
                                },
                                new OrderItem
                                {
                                    ProductId = products[2].Id,
                                    ProductName = products[2].Name,
                                    ProductPrice = products[2].Price,
                                    Quantity = 1,
                                    Product = products[2]
                                }
                            }
                        },
                        new Order
                        {
                            UserId = normalUser.Id,
                            OrderDate = DateTime.Now.AddDays(-5),
                            Status = "Shipped",
                            TotalPrice = products[1].Price,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem
                                {
                                    ProductId = products[1].Id,
                                    ProductName = products[1].Name,
                                    ProductPrice = products[1].Price,
                                    Quantity = 1,
                                    Product = products[1]
                                }
                            }
                        },
                        new Order
                        {
                            UserId = normalUser.Id,
                            OrderDate = DateTime.Now.AddDays(-2),
                            Status = "Processing",
                            TotalPrice = products[3].Price + products[4].Price,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem
                                {
                                    ProductId = products[3].Id,
                                    ProductName = products[3].Name,
                                    ProductPrice = products[3].Price,
                                    Quantity = 1,
                                    Product = products[3]
                                },
                                new OrderItem
                                {
                                    ProductId = products[4].Id,
                                    ProductName = products[4].Name,
                                    ProductPrice = products[4].Price,
                                    Quantity = 1,
                                    Product = products[4]
                                }
                            }
                        },
                        new Order
                        {
                            UserId = normalUser.Id,
                            OrderDate = DateTime.Now.AddHours(-6),
                            Status = "Pending",
                            TotalPrice = products[0].Price * 2,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem
                                {
                                    ProductId = products[0].Id,
                                    ProductName = products[0].Name,
                                    ProductPrice = products[0].Price,
                                    Quantity = 2,
                                    Product = products[0]
                                }
                            }
                        },
                        new Order
                        {
                            UserId = normalUser.Id,
                            OrderDate = DateTime.Now.AddDays(-15),
                            Status = "Cancelled",
                            TotalPrice = products[2].Price * 3,
                            OrderItems = new List<OrderItem>
                            {
                                new OrderItem
                                {
                                    ProductId = products[2].Id,
                                    ProductName = products[2].Name,
                                    ProductPrice = products[2].Price,
                                    Quantity = 3,
                                    Product = products[2]
                                }
                            }
                        }
                    };

                    await context.Orders.AddRangeAsync(orders);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
} 