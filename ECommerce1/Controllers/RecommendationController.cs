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
    public class RecommendationController(ResourceDbContext resourceDbContext, IConfiguration configuration) : Controller
    {
        const int MAX_RECOMMENDED_PRODUCTS = 20;

        /// <summary>
        /// Get recommendations for a user
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<ProductsProductViewModel>>> GetRecentlyViewedUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<RecentlyViewedItem> recentlyViewedItems = await resourceDbContext.RecentlyViewedItems.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).ThenInclude(p => p.Category).ToListAsync();

            List<Category> viewedCategories = await resourceDbContext.Categories.Where(c => recentlyViewedItems.Any(item => item.Product.Category.Id == c.Id)).Include(c => c.Products).ToListAsync();

            List<Product> recommendedProducts = [];
            if (viewedCategories.Count == 0)
            {
                return Ok(recommendedProducts);
            }
            int productPerCategory = MAX_RECOMMENDED_PRODUCTS / viewedCategories.Count;
            if (productPerCategory == 0)
            {
                productPerCategory = 1;
            }

            foreach (Category category in viewedCategories)
            {
                category.Products = category.Products.OrderByDescending(p => recentlyViewedItems.Count(item => item.Product.Id == p.Id)).Where(p => recentlyViewedItems.All(item => item.Product.Id != p.Id)).Take(productPerCategory).ToList();
            }

            foreach (Category viewedCategory in viewedCategories)
            {
                foreach (Product product in viewedCategory.Products)
                {
                    recommendedProducts.Add(product);
                }
                if (recommendedProducts.Count >= MAX_RECOMMENDED_PRODUCTS)
                {
                    break;
                }
            }

            return Ok(recommendedProducts);
        }

        /// <summary>
        /// Get general recommendations
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<ActionResult<IList<ProductsProductViewModel>>> GetRecentlyViewed()
        {
            List<Product> recommendedProducts = await resourceDbContext.Products.OrderByDescending(p => p.RecentlyViewedItems.Count).Take(MAX_RECOMMENDED_PRODUCTS).ToListAsync();

            return Ok(recommendedProducts);
        }
    }
}
