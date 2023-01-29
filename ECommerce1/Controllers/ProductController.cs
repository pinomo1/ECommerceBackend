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
    public class ProductController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        public IValidator<AddProductViewModel> ProductsValidator { get; set; }
        public BlobWorker BlobWorker { get; set; }

        public ProductController(ResourceDbContext resourceDbContext,
            IValidator<AddProductViewModel> productssValidator,
            BlobWorker blobWorker)
        {
            this.resourceDbContext = resourceDbContext;
            ProductsValidator = productssValidator;
            BlobWorker = blobWorker;
        }


        /// <summary>
        /// Gets product by its id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet("{guid}")]
        public async Task<ActionResult<Product>> GetProduct(string guid)
        {
            Product? product = await resourceDbContext.Products
                .Include(p => p.Category).Include(p => p.Seller)
                .Include(p => p.ProductPhotos)
                .FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if(product == null)
            {
                return NotFound("No such product exists");
            }
            return Ok(product);
        }

        /// <summary>
        /// Gets list of products with matching title
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="page">Pagination: current page (first by default)</param>
        /// <param name="onPage">Pagination: number of products on page</param>
        /// <param name="sorting">Sorting method</param>
        /// <returns></returns>
        [HttpGet("title/{title}")]
        public async Task<ActionResult<ProductsViewModel>> ByTitle(string? title, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => EF.Functions.Like(p.Name, $"%{title}%"))
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelByTitle viewModel = new()
            {
                Products = unorderedProducts,
                Title = title,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        /// <summary>
        /// Gets list of products by seller's id
        /// </summary>
        /// <param name="guid">Seller's id</param>
        /// <param name="page">Pagination: current page (first by default)</param>
        /// <param name="onPage">Pagination: number of products on page</param>
        /// <param name="sorting">Sorting method</param>
        /// <returns></returns>
        [HttpGet("seller/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> BySellerId(string guid, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            Seller? user = await resourceDbContext.Sellers
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (user == null)
            {
                return NotFound("No such seller exists");
            }

            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => p.Seller.Id.ToString() == guid)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelBySeller viewModel = new()
            {
                Products = unorderedProducts,
                Seller = user,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        /// <summary>
        /// Gets list of products by category's id
        /// </summary>
        /// <param name="guid">Category's id</param>
        /// <param name="page">Pagination: current page (first by default)</param>
        /// <param name="onPage">Pagination: number of products on page</param>
        /// <param name="sorting">Sorting method</param>
        /// <returns></returns>
        [HttpGet("category/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> ByCategoryId(string guid, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }

            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => p.Category.Id.ToString() == guid)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelByCategory viewModel = new()
            {
                Products = unorderedProducts,
                Category = category,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        [NonAction]
        private async Task<IEnumerable<ProductsProductViewModel>> PrepareProducts(IQueryable<ProductsProductViewModel> unorderedProducts, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);
            if (page > totalPages)
            {
                page = totalPages;
            }
            if (page <= 0)
            {
                page = 1;
            }
            if (onPage > 50)
            {
                onPage = 50;
            }
            if (onPage < 5)
            {
                onPage = 5;
            }

            IOrderedQueryable<ProductsProductViewModel> orderedProducts;

            switch (sorting)
            {
                case ProductSorting.OlderFirst:
                    orderedProducts = unorderedProducts.OrderBy(p => p.CreationTime);
                    break;
                case ProductSorting.NewerFirst:
                    orderedProducts = unorderedProducts.OrderByDescending(p => p.CreationTime);
                    break;
                case ProductSorting.CheaperFirst:
                    orderedProducts = unorderedProducts.OrderBy(p => p.Price);
                    break;
                case ProductSorting.ExpensiveFirst:
                    orderedProducts = unorderedProducts.OrderByDescending(p => p.Price);
                    break;
                default:
                    throw new Exception();
            }

            return orderedProducts.Skip((page - 1) * onPage).Take(onPage);
        }
        
        /// <summary>
        /// Add product, must be logged in as a seller
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddMainCategory([FromForm] AddProductViewModel product)
        {
            var resultValid = await ProductsValidator.ValidateAsync(product);

            string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Seller? seller = await resourceDbContext.Sellers.FirstOrDefaultAsync(p => p.AuthId.ToString() == id);
            if (seller == null)
            {
                return BadRequest("No such seller exists");
            }
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == product.CategoryId);
            if(category == null)
            {
                return BadRequest("No such category exists");
            }
            if (category.AllowProducts == false)
            {
                return BadRequest("This category does not allow products");
            }

            IEnumerable<string> references = await BlobWorker.AddPublicationPhotos(product.Photos);
            if (references.Count() == 0)
            {
                this.ModelState.AddModelError("Error", "Photos has not been uploaded");
                return BadRequest(this.ModelState);
            }
            
            List<ProductPhoto> productPhotos = new();
            foreach (string reference in references)
            {
                productPhotos.Add(new ProductPhoto()
                {
                    Url = reference
                });
            }

            Product prod = new()
            {
                Name = product.Name,
                CreationTime = DateTime.UtcNow,
                Description = product.Description,
                Price = product.Price,
                Category = category,
                Seller = seller,
                ProductPhotos = productPhotos
            };
            await resourceDbContext.Products.AddAsync(prod);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Edit specific product, muse be seller of that product or admin
        /// </summary>
        /// <param name="guid">Id of a product</param>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPut("edit/{guid}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> EditAsync(string guid, [FromForm] AddProductViewModel product)
        {
            Product? prod = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if(User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != prod.Seller.AuthId)
            {
                return BadRequest();
            }

            if (prod == null)
            {
                return BadRequest("No product with such id exists");
            }
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == product.CategoryId);
            if (category == null)
            {
                return BadRequest("No such category exists");
            }
            
            prod.Name = product.Name;
            prod.Description = product.Description;
            prod.Price = product.Price;
            prod.Category = category;
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Delete specific product, must be seller of that product or admin
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpDelete("delete/{guid}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> DeleteAsync(string guid)
        {
            Product? product = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != product.Seller.AuthId)
            {
                return BadRequest();
            }

            if (product == null)
            {
                return BadRequest("No product with such id exists");
            }
            resourceDbContext.Products.Remove(product);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
