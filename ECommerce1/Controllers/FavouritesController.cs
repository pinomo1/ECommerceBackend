using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public FavouritesController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        /// <summary>
        /// Get all favourite items
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<ProductsProductViewModel>>> GetFavourites()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<FavouriteItem> favItems = await resourceDbContext.FavouriteItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ToListAsync();
            List<ProductsProductViewModel> favItemsViewModel = new();
            foreach (var item in favItems)
            {
                Product p = item.Product;
                favItemsViewModel.Add(new ProductsProductViewModel
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price,
                    OrderCount = p.Orders.Count,
                    Rating = p.Reviews.Count == 0 ? 0 : p.Reviews.Average(r => r.Quality)
                });
            }

            return Ok(favItemsViewModel);
        }

        /// <summary>
        /// Add item to favourite list
        /// </summary>
        /// <param name="guid">Product's ID</param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToFavourites(string guid)
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
            if (await resourceDbContext.FavouriteItems.AnyAsync(ci => ci.User.AuthId == userId && ci.Product.Id == product.Id))
            {
                return BadRequest(new { error_message = "Item already in favourite list" });
            }
            FavouriteItem item = new()
            {
                User = user,
                Product = product
            };
            await resourceDbContext.FavouriteItems.AddAsync(item);
            await resourceDbContext.SaveChangesAsync();
            return Ok(item.Id);
        }

        /// <summary>
        /// Remove item from favourite list
        /// </summary>
        /// <param name="guid">Product's ID, not FavouriteItem's</param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFromFavourites(string guid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (user == null)
            {
                return BadRequest(new { error_message = "User not found" });
            }
            FavouriteItem? favouriteItem = await resourceDbContext.FavouriteItems.FirstOrDefaultAsync(p => p.Product.Id.ToString() == guid && p.User.AuthId == userId);
            if (favouriteItem == null)
            {
                return NotFound(new { error_message = "No such product" });
            }
            resourceDbContext.FavouriteItems.Remove(favouriteItem);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
