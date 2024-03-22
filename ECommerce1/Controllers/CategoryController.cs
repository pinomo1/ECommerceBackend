using ECommerce1.Extensions;
using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ResourceDbContext resourceDbContext, BlobWorker blobWorker, TranslationService translationService) : ControllerBase
    {
        public BlobWorker BlobWorker { get; set; } = blobWorker;
        public TranslationService TranslationService { get; set; } = translationService;

        /// <summary>
        /// Adds main category (main category is a category that has no parent category)
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost("add/main")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMainCategory(string category)
        {
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.ToLower().Trim());
            if(foundCategory != null)
            {
                return BadRequest(new
                {
                    error_message = "CategoryName with such name already exists"
                });
            }
            Category newCategory = new()
            {
                AllowProducts = false,
                Name = category,
                ParentCategory = null,
                ImageUrl = "",
                IsSearchable = false
            };
            await resourceDbContext.Categories.AddAsync(newCategory);
            await resourceDbContext.SaveChangesAsync();
            await TranslationService.AddTranslation(TranslatedObjectType.CategoryName, newCategory.Id.ToString(), "en", category);
            return Ok(newCategory.Id);
        }

        /// <summary>
        /// Adds sub category (sub category is a category that has parent category)
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost("add/sub")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSubCategory(AddCategoryViewModel category)
        {
            if (category.ParentCategoryId == null)
            {
                return BadRequest(new {error_message = "No parent category with such id was found"});
            }
            Category? parentCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == category.ParentCategoryId.ToString());
            if (parentCategory  == null)
            {
                return BadRequest(new
                {
                    error_message = "No parent category with such id was found"
                });
            }
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim());
            if (foundCategory != null)
            {
                return BadRequest(new
                {
                    error_message = "CategoryName with such name already exists"
                });
            }
            Category newCategory = new()
            {
                AllowProducts = category.AllowProducts,
                Name = category.Name,
                ParentCategory = parentCategory,
                ImageUrl = "",
                IsSearchable = category.AllowProducts
            };
            await resourceDbContext.Categories.AddAsync(newCategory);
            await resourceDbContext.SaveChangesAsync();
            await TranslationService.AddTranslation(TranslatedObjectType.CategoryName, newCategory.Id.ToString(), "en", category.Name);
            return Ok(newCategory.Id);
        }

        [NonAction]
        private async Task<Category> TranslateCategory(Category category)
        {
            string translation = await TranslationService.Translate(TranslatedObjectType.CategoryName, category.Id.ToString(), HttpContext);
            if (translation != null)
            {
                category.Name = translation;
            }
            return category;
        }

        [NonAction]
        private async Task<List<Category>> TranslateCategories(List<Category> categories)
        {
            List<Category> translatedCategories = [];
            foreach (var category in categories)
            {
                translatedCategories.Add(await TranslateCategory(category));
            }
            return categories;
        }

        [HttpPost("add/translation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTranslation(string id, string locale, string name)
        {
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == id);
            if (category == null)
            {
                return BadRequest(new
                {
                    error_message = "No category with such id was found"
                });
            }
            await TranslationService.AddTranslation(TranslatedObjectType.CategoryName, id, locale, name);
            return Ok();
        }

        /// <summary>
        /// Returns all main categories (categories that have no parent category) (example of usage: main page)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<Category>> GetMainCategories()
        {
            var categories = await resourceDbContext.Categories.Where(c => c.ParentCategory == null).Include(c => c.ChildCategories).ToListAsync();
            return await TranslateCategories(categories);
        }

        /// <summary>
        /// Returns category by id (if category allows products, redirects to products controller, else redirects to sub categories controller)
        /// </summary>
        /// <param name="guid">Parent category's id</param>
        /// <returns></returns>
        [HttpGet("{guid}")]
        [Obsolete("Use below methods instead")]
        public async Task<IActionResult> GetCategory(string guid)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }
            return category.AllowProducts ? RedirectToAction("ByCategoryId", "Product", new { guid }) : RedirectToAction("GetSubCategories", "CategoryName", new { guid });
        }

        /// <summary>
        /// Returns list of categories by title (example of usage: search)
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet("title/{title}")]
        public async Task<IActionResult> GetCategoriesByTitle(string title)
        {
            var categories = await resourceDbContext.Categories.Where(p => EF.Functions.Like(p.Name, $"%{title}%")).ToListAsync();
            return Ok(await TranslateCategories(categories));
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllCategories()
        {
            List<Category> mainCategories = await resourceDbContext.Categories.Where(c => c.AllowProducts == false).ToListAsync();
            mainCategories = await TranslateCategories(mainCategories);
            List<Category> subCategories = await resourceDbContext.Categories.Where(c => c.AllowProducts == true).ToListAsync();
            subCategories = await TranslateCategories(subCategories);

            AllCategoriesResponse allCategoriesResponse = new()
            {
                MainCategories = mainCategories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    ParentId = (c.ParentCategory == null ? Guid.Empty : c.ParentCategory.Id),
                    Name = c.Name,
                    AllowProducts = c.AllowProducts,
                    ImageUrl = c.ImageUrl,
                    IsSearchable = c.IsSearchable
                })
                .ToArray(),

                SubCategories = subCategories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    ParentId = (c.ParentCategory == null ? Guid.Empty : c.ParentCategory.Id),
                    Name = c.Name,
                    AllowProducts = c.AllowProducts,
                    ImageUrl = c.ImageUrl,
                    IsSearchable = c.IsSearchable
                }).ToArray()
            };

            return Ok(allCategoriesResponse);
        }

        /// <summary>
        /// Returns sub categories of category with id = guid
        /// </summary>
        /// <param name="guid">Parent category's id</param>
        /// <returns></returns>
        [HttpGet("sub/{guid}")]
        [Obsolete("Use above methods instead")]
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

        /// <summary>
        /// Deletes category with id = guid
        /// </summary>
        /// <param name="guid">Category's id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Edits category with id = guid
        /// </summary>
        /// <param name="guid">Category's id</param>
        /// <param name="category"></param>
        /// <returns></returns>
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
            foundCategory.IsSearchable = category.AllowProducts;
            await resourceDbContext.SaveChangesAsync();
            return Ok(foundCategory.Id);
        }

        [HttpPatch("edit/image/{guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCategoryImage(string guid, IFormFile image)
        {
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (foundCategory == null)
            {
                return NotFound("No such category exists");
            }
            string? reference = await BlobWorker.AddPublicationPhoto(image);
            if (reference == String.Empty)
            {
                return BadRequest(new
                {
                    error_message = "Bad photo"
                });
            }
            foundCategory.ImageUrl = reference;
            await resourceDbContext.SaveChangesAsync();
            return Ok(foundCategory.Id);
        }
    }
}
