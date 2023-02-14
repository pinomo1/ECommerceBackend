using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public CartController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        /// <summary>
        /// Get all items in cart
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<CartItemViewModel>>> GetCart()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<CartItem> cartItems = await resourceDbContext.CartItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ToListAsync();
            List<CartItemViewModel> cartItemViewModels = new();
            foreach (var group in cartItems.GroupBy(ci => ci.Product))
            {
                cartItemViewModels.Add(new CartItemViewModel
                {
                    Product = group.Key,
                    Quantity = group.Count()
                });
            }

            return Ok(cartItemViewModels);
        }

        /// <summary>
        /// Change quantity of specified product in cart
        /// </summary>
        /// <param name="guid">Product GUID</param>
        /// <param name="quantity">Quantity desired in total</param>
        /// <returns></returns>
        [HttpPost("change")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangeQuantity(string guid, int quantity)
        {
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if (product == null)
            {
                return BadRequest(new { error_message = "No such product" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (user == null)
            {
                return BadRequest(new { error_message = "User not found" });
            }
            int inCartQuantityNow = await resourceDbContext.CartItems.CountAsync(ci => ci.Product.Id.ToString() == guid && ci.User.AuthId == userId);
            if(quantity > 9)
            {
                return BadRequest(new { error_message = "Max quantity is 9" });
            }
            if(quantity < 0)
            {
                return BadRequest(new { error_message = "Quantity cannot be less than 0" });
            }
            int difference = quantity - inCartQuantityNow;
            if(difference == 0)
            {
                return NoContent();
            }
            else if(difference > 0)
            {
                List<CartItem> cartItems = new();
                for (int i = 0; i < difference; i++)
                {
                    cartItems.Add(new() { User = user, Product = product });
                }
                await resourceDbContext.CartItems.AddRangeAsync(cartItems);
                await resourceDbContext.SaveChangesAsync();
                return Ok();
            }
            else
            {
                difference = -difference;
                for (int i = 0; i < difference; i++)
                {
                    List<CartItem> cartItems = (List<CartItem>)resourceDbContext.CartItems.Where(ci => ci.Product.Id.ToString() == guid && ci.User.AuthId == userId);
                    resourceDbContext.CartItems.RemoveRange(cartItems);
                    await resourceDbContext.SaveChangesAsync();
                }
                return Ok();
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Obsolete("Use ChageQuantity (change/{guid}) instead")]
        [HttpPost("add/{guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToCart(string guid)
        {
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest(new { error_message = "No such product" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if(user == null)
            {
                return BadRequest(new { error_message = "User not found" });
            }
            CartItem item = new()
            {
                User = user,
                Product = product
            };
            await resourceDbContext.CartItems.AddAsync(item);
            await resourceDbContext.SaveChangesAsync();
            return Ok(item.Id);
        }


        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="guid">Product's ID, not CartItem's</param>
        /// <returns></returns>
        [Obsolete("Use ChageQuantity (change/{guid}) instead")]
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFromCart(string guid)
        {
            CartItem? cartItem = await resourceDbContext.CartItems.FirstOrDefaultAsync(p => p.Product.Id.ToString() == guid);
            if (cartItem == null)
            {
                return NotFound(new { error_message = "No such product" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if(userId != cartItem.User.AuthId)
            {
                return BadRequest(new
                {
                    error_message = "You are not authorized to remove this item"
                });
            }
            resourceDbContext.CartItems.Remove(cartItem);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
