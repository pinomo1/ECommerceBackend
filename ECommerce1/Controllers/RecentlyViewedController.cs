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
    public class RecentlyViewedController : Controller
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public RecentlyViewedController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        /// <summary>
        /// Get all recently viewed items
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<ProductsProductViewModel>>> GetRecentlyViewed()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<RecentlyViewedItem> recentlyViewedItems = await resourceDbContext.RecentlyViewedItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ToListAsync();
            List<ProductsProductViewModel> favItemsViewModel = new();
            foreach (var item in recentlyViewedItems)
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

            favItemsViewModel.Reverse();

            return Ok(favItemsViewModel);
        }

        /// <summary>
        /// Remove item from recently viewed list
        /// </summary>
        /// <param name="guid">Product's ID, not RecentlyViewedItem's ID</param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveFromRecentlyViewed(string guid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (user == null)
            {
                return BadRequest(new { error_message = "User not found" });
            }
            RecentlyViewedItem? recentlyViewedItem = await resourceDbContext.RecentlyViewedItems.FirstOrDefaultAsync(p => p.Product.Id.ToString() == guid && p.User.AuthId == userId);
            if (recentlyViewedItem == null)
            {
                return NotFound(new { error_message = "No such product" });
            }
            resourceDbContext.RecentlyViewedItems.Remove(recentlyViewedItem);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("deleteall")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveAllFromRecentlyViewed()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<RecentlyViewedItem> recentlyViewedItems = await resourceDbContext.RecentlyViewedItems.Where(ci => ci.User.AuthId == userId).ToListAsync();
            resourceDbContext.RecentlyViewedItems.RemoveRange(recentlyViewedItems);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
