using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public OrderController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }

        [HttpGet("getOwn")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<Order>>> GetOrders()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Order> cartItems = await resourceDbContext.Orders.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).OrderByDescending(o => o.OrderTime).ToListAsync();
            return Ok(cartItems);
        }

        [HttpGet("getOwnSeller")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<IList<Order>>> GetOrdersSeller()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Order> cartItems = await resourceDbContext.Orders.Where(ci => ci.Product.Seller.AuthId == userId).Include(ci => ci.Product).OrderByDescending(o => o.OrderTime).ToListAsync();
            return Ok(cartItems);
        }

        [HttpPost("addNow/{guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddOrderNow(string guid, string addressGuid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if(profile == null)
            {
                return BadRequest(new { error_message = "No such profile was found" });
            }
            Address? address = await resourceDbContext.Addresses.Include(a => a.City).ThenInclude(c => c.Country).FirstOrDefaultAsync(a => a.User.AuthId == userId && a.Id.ToString() == addressGuid);
            if(address == null)
            {
                return BadRequest(new { error_message = "No such address was found" });
            }
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest(new { error_message = "No such product was found" });
            }
            Order order = new()
            {
                AddressCopy = address.Normalize(),
                OrderTime = DateTime.Now,
                Product = product,
                User = profile
            };

            await resourceDbContext.Orders.AddAsync(order);
            await resourceDbContext.SaveChangesAsync();

            return Ok(order.Id);
        }

        [HttpPost("addFromCart")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddFromCart(string addressGuid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (profile == null)
            {
                return BadRequest(new { error_message = "No such profile was found" });
            }
            Address? address = await resourceDbContext.Addresses.Include(a => a.City).ThenInclude(c => c.Country).FirstOrDefaultAsync(a => a.User.AuthId == userId && a.Id.ToString() == addressGuid);
            if (address == null)
            {
                return BadRequest(new { error_message = "No such address was found" });
            }
            IList<CartItem>? products = await resourceDbContext.CartItems.Include(ci => ci.Product).Where(p => p.User.AuthId.ToString() == userId).ToListAsync();
            if (products == null)
            {
                return BadRequest(new { error_message = "No product were found" });
            }
            List<Order> orders = new();

            foreach (var item in products)
            {
                orders.Add(new()
                {
                    AddressCopy = address.Normalize(),
                    OrderTime = DateTime.Now,
                    Product = item.Product,
                    User = profile
                });
            }

            await resourceDbContext.Orders.AddRangeAsync(orders);
            resourceDbContext.CartItems.RemoveRange(products);
            await resourceDbContext.SaveChangesAsync();

            return Ok(orders.Select(o => new { id = o.Id }));
        }
    }
}
