using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController(ResourceDbContext resourceDbContext) : ControllerBase
    {
        private const int maxProductInCart = 99;

        /// <summary>
        /// Return maximum quantity of certain product in the cart
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_max")]
        public IActionResult GetMax()
        {
            return Ok(new { max = maxProductInCart });
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
            List<CartItem> cartItems = await resourceDbContext.CartItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ThenInclude(ci => ci.ProductPhotos).ToListAsync();
            List<CartItemViewModel> cartItemViewModels = [];

            foreach (CartItem cartItem in cartItems)
            {
                CartItemViewModel cartItemViewModel = new()
                {
                    Product = cartItem.Product,
                    Quantity = cartItem.Quantity
                };
                cartItemViewModels.Add(cartItemViewModel);
            }

            return Ok(cartItemViewModels);
        }

        public struct CartItemsGroupedBySeller
        {
            public Seller Seller { get; set; }
            public IList<CartItemViewModel> CartItems { get; set; }
        }

        /// <summary>
        /// Get all items in cart grouped by seller
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own_by_seller")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<CartItemsGroupedBySeller>>> GetCartBySeller()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<CartItem> cartItems = await resourceDbContext.CartItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ThenInclude(ci => ci.ProductPhotos).ToListAsync();
            List<CartItemViewModel> cartItemViewModels = [];
            
            foreach (CartItem cartItem in cartItems)
            {
                CartItemViewModel cartItemViewModel = new()
                {
                    Product = cartItem.Product,
                    Quantity = cartItem.Quantity
                };
                cartItemViewModels.Add(cartItemViewModel);
            }

            List<CartItemsGroupedBySeller> cartItemsGroupedBySellers = [];
            foreach (CartItemViewModel cartItemViewModel in cartItemViewModels)
            {
                Seller? seller = await resourceDbContext.Sellers.FirstOrDefaultAsync(s => s.Id == cartItemViewModel.Product.Seller.Id);
                if (seller == null)
                {
                    return BadRequest(new { error_message = "Seller not found" });
                }
                CartItemsGroupedBySeller cartItemsGroupedBySeller = cartItemsGroupedBySellers.FirstOrDefault(cigs => cigs.Seller.Id == seller.Id);
                if (cartItemsGroupedBySeller.Seller == null)
                {
                    cartItemsGroupedBySeller.Seller = seller;
                    cartItemsGroupedBySeller.CartItems =
                    [
                        cartItemViewModel
                    ];
                    cartItemsGroupedBySellers.Add(cartItemsGroupedBySeller);
                }
                else
                {
                    cartItemsGroupedBySeller.CartItems.Add(cartItemViewModel);
                }
            }

            return Ok(cartItemsGroupedBySellers);
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
            if(quantity > maxProductInCart)
            {
                quantity = maxProductInCart;
            }
            if(quantity < 0)
            {
                return BadRequest(new { error_message = "Quantity cannot be less than 0" });
            }
            if(quantity == 0)
            {
                CartItem? cartItem = await resourceDbContext.CartItems.FirstOrDefaultAsync(ci => ci.Product.Id.ToString() == guid && ci.User.AuthId == userId);
                if(cartItem == null)
                {
                    return BadRequest(new { error_message = "No such item in cart" });
                }
                resourceDbContext.CartItems.Remove(cartItem);
                await resourceDbContext.SaveChangesAsync();
                return Ok();
            }
            if(inCartQuantityNow == 0)
            {
                CartItem cartItem = new()
                {
                    Product = product,
                    User = user,
                    Quantity = quantity
                };
                await resourceDbContext.CartItems.AddAsync(cartItem);
                await resourceDbContext.SaveChangesAsync();
                return Ok();
            }
            CartItem? cartItemNow = await resourceDbContext.CartItems.FirstOrDefaultAsync(ci => ci.Product.Id.ToString() == guid && ci.User.AuthId == userId);
            if(cartItemNow == null)
            {
                return BadRequest(new { error_message = "No such item in cart" });
            }
            cartItemNow.Quantity = quantity;
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Change quantity of specified product in cart
        /// </summary>
        /// <param name="guids">Product GUIDs</param>
        /// <returns></returns>
        [HttpDelete("deleteSelected")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteSelected(IList<string> guids)
        {
            List<Product> products = [];
            foreach (string guid in guids)
            {
                Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
                if (product == null)
                {
                    return BadRequest(new { error_message = "No such product" });
                }
                products.Add(product);
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (user == null)
            {
                return BadRequest(new { error_message = "User not found" });
            }
            foreach (Product product in products)
            {
                CartItem? cartItem = await resourceDbContext.CartItems.FirstOrDefaultAsync(ci => ci.Product.Id.ToString() == product.Id.ToString() && ci.User.AuthId == userId);
                if (cartItem == null)
                {
                    return BadRequest(new { error_message = "No such item in cart" });
                }
                resourceDbContext.CartItems.Remove(cartItem);
            }
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
