using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public CartController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        [HttpGet("get_own")]
        [Authorize(Roles="User")]
        public async Task<IActionResult> GetCart()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<CartItem> cartItems = await resourceDbContext.CartItems.Where(ci => ci.User.AuthId == userId).ToListAsync();
            return Ok(cartItems);
        }

        [HttpPost("add/{guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToCart(string guid)
        {
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest("No such product");
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId.ToString() == userId);
            if(user == null)
            {
                return BadRequest("User not found");
            }
            CartItem item = new()
            {
                User = user,
                Product = product
            };
            await resourceDbContext.CartItems.AddAsync(item);
            await resourceDbContext.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFromCart(string guid)
        {
            CartItem? cartItem = await resourceDbContext.CartItems.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if (cartItem == null)
            {
                return NotFound("No such product");
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId.ToString() == userId);
            if(userId != cartItem.User.AuthId)
            {
                return BadRequest("You are not authorized to remove this item");
            }
            resourceDbContext.CartItems.Remove(cartItem);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
