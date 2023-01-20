using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public CategoryController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }

        [HttpPost("add/main")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMainCategory(AddCategoryViewModel category)
        {
            Category? parentCategoty = await this.resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId);
            if(category.ParentCategoryId != null)
            {
                return RedirectToAction("AddSubCategory", "Category", new { category });
            }
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim());
            if(foundCategory != null)
            {
                return BadRequest("Category with such name already exists");
            }
            Category newCategory = new()
            {
                AllowProducts = category.AllowProducts,
                Name = category.Name,
                ParentCategory = null
            };
            await resourceDbContext.Categories.AddAsync(newCategory);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("add/sub")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSubCategory(AddCategoryViewModel category)
        {
            if (category.ParentCategoryId == null)
            {
                return BadRequest();
            }
            Category? parentCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == category.ParentCategoryId.ToString());
            if (parentCategory  == null)
            {
                return BadRequest("No parent category with such id was found");
            }
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim());
            if (foundCategory != null)
            {
                return BadRequest("Category with such name already exists");
            }
            Category newCategory = new()
            {
                AllowProducts = category.AllowProducts,
                Name = category.Name,
                ParentCategory = parentCategory
            };
            await resourceDbContext.Categories.AddAsync(newCategory);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<Category>> GetMainCategories()
        {
            var categories = await resourceDbContext.Categories.Where(c => c.ParentCategory == null).ToListAsync();
            return categories;
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetCategory(string guid)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }
            return category.AllowProducts ? RedirectToAction("ByCategoryId", "Product", new { guid }) : RedirectToAction("GetSubCategories", "Category", new { guid });
        }

        [HttpGet("sub/{guid}")]
        public async Task<ActionResult<Category>> GetSubCategories(string guid)
        {
            var category = await resourceDbContext.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if(category == null)
            {
                return NotFound("No such category exists");
            }
            return Ok(category);
        }

        [HttpDelete("delete/{guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(string guid)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }
            resourceDbContext.Categories.Remove(category);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("edit/{guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCategory(string guid, AddCategoryViewModel category)
        {
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (foundCategory == null)
            {
                return NotFound("No such category exists");
            }
            foundCategory.Name = category.Name;
            foundCategory.AllowProducts = category.AllowProducts;
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
