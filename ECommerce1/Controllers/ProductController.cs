using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;
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
        /// Returns elements of ProductSorting enum
        /// </summary>
        /// <returns></returns>
        [HttpGet("sorting")]
        public async Task<ActionResult<IList<string>>> GetSortingEnum()
        {
            IDictionary<int, string> names = Enum.GetNames(typeof(ProductSorting)).ToList().Select((s, i) => new { s, i }).ToDictionary(x => x.i + 1, x => x.s);
            
            List<TempStruct111> tsList = new();

            foreach (var item in names)
            {
                string val = item.Value.Replace("F", " f");
                tsList.Add(new() { Key = item.Key, Value = val });
            }

            return Ok(tsList);
        }

        public struct TempStruct111
        {
            public int Key { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Gets product by its id
        /// </summary>
        /// <param name="guid">Product ID</param>
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
                return NotFound(new { error_message = "No such product exists" } );
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
        /// <param name="fromPrice">Minimum price</param>
        /// <param name="toPrice">Maximum price</param>
        /// <param name="inStock">true to output only in stock. false to output everyone</param>
        /// <returns></returns>
        [HttpGet("title")]
        public async Task<ActionResult<ProductsViewModel>> ByTitle(string? title, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.PopularFirst, int fromPrice = 0, int toPrice = 100000, bool inStock = false)
        {
            IList<ProductsProductViewModel> unorderedProducts = await resourceDbContext.Products
                .Where(p => (title == null ? true : EF.Functions.Like(p.Name, $"%{title}%")) && p.Price >= fromPrice && p.Price <= toPrice && (inStock == true ? inStock == p.InStock : true))
                .Include(p => p.Reviews)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price,
                    OrderCount = p.Orders.Count,
                    Rating = p.Reviews.Count == 0 ? 0 : p.Reviews.Average(r => r.Quality),
                    InStock = p.InStock
                }).ToListAsync();
            
            if (onPage > 50)
            {
                onPage = 50;
            }
            else if (onPage < 1)
            {
                onPage = 1;
            }

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;
            decimal minPrice, maxPrice;

            try
            {
                var preparation = await PrepareProducts(unorderedProducts, page, onPage, sorting);
                products = preparation.Products;
                minPrice = preparation.MinPrice;
                maxPrice = preparation.MaxPrice;
            }
            catch (Exception)
            {
                return NotFound(new { error_message = "Sorting error occurred!" });
            }

            ProductsViewModelByTitle viewModel = new()
            {
                Products = products,
                Title = title,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return Ok(viewModel);
        }


        /// <summary>
        /// Gets list of products by seller's id
        /// </summary>
        /// <param name="guid">Seller's id</param>
        /// <param name="title">Additional title of a product</param>
        /// <param name="page">Pagination: current page (first by default)</param>
        /// <param name="onPage">Pagination: number of products on page</param>
        /// <param name="sorting">Sorting method</param>
        /// <param name="fromPrice">Minimum price</param>
        /// <param name="toPrice">Maximum price</param>
        /// <param name="inStock">true to output only in stock. false to output everyone</param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("seller/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> BySellerId(string guid, string? title, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.PopularFirst, int fromPrice = 0, int toPrice = 100000, bool inStock = false, string? categoryId = null)
        {
            Seller? user = await resourceDbContext.Sellers
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);

            if (user == null)
            {
                return NotFound(new { error_message = "No such seller exists" });
            }

            if (onPage > 50)
            {
                onPage = 50;
            }
            else if (onPage < 1)
            {
                onPage = 1;
            }

            if(categoryId != null)
            {
                Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == categoryId);
                if(category == null)
                {
                    return NotFound(new { error_message = "No such category exists" });
                }
                if(category.AllowProducts == false)
                {
                    return BadRequest(new { error_message = "No products here" });
                }
            }

            IList<ProductsProductViewModel> unorderedProducts = await resourceDbContext.Products
                .Where(p => p.Seller.Id.ToString() == guid && p.Price >= fromPrice && p.Price <= toPrice && (title == null ? true : EF.Functions.Like(p.Name, $"%{title}%")) && (inStock == true ? inStock == p.InStock : true) && (categoryId != null ? p.Category.Id.ToString() == categoryId : true))
                .Include(p => p.Reviews)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price,
                    OrderCount = p.Orders.Count,
                    Rating = p.Reviews.Count == 0 ? 0 : p.Reviews.Average(r => r.Quality),
                    InStock = p.InStock
                }).ToListAsync();

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;
            decimal minPrice, maxPrice;

            try
            {
                var preparation = await PrepareProducts(unorderedProducts, page, onPage, sorting);
                products = preparation.Products;
                minPrice = preparation.MinPrice;
                maxPrice = preparation.MaxPrice;
            }
            catch (Exception)
            {
                return NotFound(new
                {
                    error_message = "Sorting error occurred!"
                });
            }

            ProductsViewModelBySeller viewModel = new()
            {
                Products = products,
                Seller = user,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return Ok(viewModel);
        }

        [NonAction]
        public async Task<IList<ProductsProductViewModel>> GetSubcategoriesAndProducts(string guid, int fromPrice, int toPrice, bool inStock, string? title)
        {
            Category? category = await resourceDbContext.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);

            if (category == null)
            {
                return new List<ProductsProductViewModel>();
            }

            IList<ProductsProductViewModel> unorderedProducts = await resourceDbContext.Products
                .Where(p => p.Category.Id.ToString() == guid && p.Price >= fromPrice && p.Price <= toPrice && (title == null ? true : EF.Functions.Like(p.Name, $"%{title}%")) && (inStock == true ? inStock == p.InStock : true))
                .Include(p => p.Reviews)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price,
                    OrderCount = p.Orders.Count,
                    Rating = p.Reviews.Count == 0 ? 0 : p.Reviews.Average(r => r.Quality),
                    InStock = p.InStock
                }).ToListAsync();

            foreach (Category subcategory in category.ChildCategories)
            {
                unorderedProducts = unorderedProducts.Concat(await GetSubcategoriesAndProducts(subcategory.Id.ToString(), fromPrice, toPrice, inStock, title)).ToList();
            }

            return unorderedProducts;
        }

        /// <summary>
        /// Gets list of products by category's id
        /// </summary>
        /// <param name="guid">Category's id</param>
        /// <param name="title">Additional title of a product</param>
        /// <param name="page">Pagination: current page (first by default)</param>
        /// <param name="onPage">Pagination: number of products on page</param>
        /// <param name="sorting">Sorting method</param>
        /// <param name="fromPrice">Minimum price</param>
        /// <param name="toPrice">Maximum price</param>
        /// <param name="inStock">true to output only in stock. false to output everyone</param>
        /// <returns></returns>
        [HttpGet("category/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> ByCategoryId(string guid, string? title, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.PopularFirst, int fromPrice = 0, int toPrice = 100000, bool inStock = false)
        {
            Category? category = await resourceDbContext.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound(new
                {
                    error_message = "No such category exists"
                });
            }

            if(onPage > 50)
            {
                onPage = 50;
            }
            else if (onPage < 1)
            {
                onPage = 1;
            }

            // Obsolete piece of code
            /*
            if (!category.AllowProducts)
            {
                return RedirectToAction("GetSubCategories", "Category", new { guid });
            }
            */

            IList<ProductsProductViewModel> unorderedProducts = new List<ProductsProductViewModel>();

            unorderedProducts = unorderedProducts.Concat(await GetSubcategoriesAndProducts(guid, fromPrice, toPrice, inStock, title)).ToList();

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;
            decimal minPrice, maxPrice;

            try
            {
                var preparation = await PrepareProducts(unorderedProducts, page, onPage, sorting);
                products = preparation.Products;
                minPrice = preparation.MinPrice;
                maxPrice = preparation.MaxPrice;
            }
            catch (Exception)
            {
                return NotFound(new
                {
                    error_message = "Sorting error occurred!"
                });
            }

            ProductsViewModelByCategory viewModel = new()
            {
                Products = products,
                Category = category,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return Ok(viewModel);
        }

        [NonAction]
        private async Task<ProductPreparation> PrepareProducts(IList<ProductsProductViewModel> unorderedProducts, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);
            decimal maxPrice = 0, minPrice = 0;
            try
            {
                maxPrice = unorderedProducts.Max(p => p.Price);
                minPrice = unorderedProducts.Min(p => p.Price);
            }
            catch (Exception)
            {

            }
            if (page > totalPages)
            {
                page = totalPages;
            }
            if (page <= 0)
            {
                page = 1;
            }
            /*
            if (onPage > 50)
            {
                onPage = 50;
            }
            if (onPage < 5)
            {
                onPage = 5;
            }
            */

            IOrderedEnumerable<ProductsProductViewModel> orderedProducts = sorting switch
            {
                ProductSorting.OlderFirst => unorderedProducts.OrderBy(p => p.CreationTime),
                ProductSorting.NewerFirst => unorderedProducts.OrderByDescending(p => p.CreationTime),
                ProductSorting.CheaperFirst => unorderedProducts.OrderBy(p => p.Price),
                ProductSorting.ExpensiveFirst => unorderedProducts.OrderByDescending(p => p.Price),
                ProductSorting.PopularFirst => unorderedProducts.OrderByDescending(p => p.OrderCount),
                ProductSorting.BestFirst => unorderedProducts.OrderByDescending(p => p.Rating),
                _ => unorderedProducts.OrderByDescending(p => p.OrderCount),
            };
            return new ProductPreparation(minPrice, maxPrice, orderedProducts.Skip((page - 1) * onPage).Take(onPage));
        }

        /// <summary>
        /// Class for preparing products
        /// </summary>
        class ProductPreparation
        {
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public IEnumerable<ProductsProductViewModel> Products { get; set; }

            public ProductPreparation(decimal min, decimal max, IEnumerable<ProductsProductViewModel> prods)
            {
                MinPrice = min;
                MaxPrice = max;
                Products = prods;
            }
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
            Seller? seller = await resourceDbContext.Sellers.FirstOrDefaultAsync(p => p.AuthId == id);
            if (seller == null)
            {
                return BadRequest(new { errror_message = "No such seller exists" });
            }
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == product.CategoryId);
            if(category == null)
            {
                return BadRequest(new
                {
                    error_message = "No such category exists"
                });
            }
            if (category.AllowProducts == false)
            {
                return BadRequest(new
                {
                    error_message = "This category does not allow products"
                });
            }

            IEnumerable<string> references = await BlobWorker.AddPublicationPhotos(product.Photos);
            if (references.Count() == 0)
            {
                return BadRequest(new { error_message = "Photos has not been uploaded" });
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
            return Ok(prod.Id);
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

            if (prod == null)
            {
                return BadRequest(new
                {
                    error_message = "No product with such id exists"
                });
            }

            if (User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != prod.Seller.AuthId)
            {
                return BadRequest( new {error_message = "Not your product"});
            }

            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == product.CategoryId);
            if (category == null)
            {
                return BadRequest(new
                {
                    error_message = "No such category exists"
                });
            }
            
            prod.Name = product.Name;
            prod.Description = product.Description;
            prod.Price = product.Price;
            prod.Category = category;
            await resourceDbContext.SaveChangesAsync();
            return Ok(prod.Id);
        }

        /// <summary>
        /// Edit stock of a product, must be seller of that product or admin
        /// </summary>
        /// <param name="guid">Product ID</param>
        /// <param name="inStock">Boolean, is in stock or not</param>
        /// <returns></returns>
        [HttpPatch("edit/{guid}/stock")]
        public async Task<IActionResult> EditStockAsync(string guid, bool inStock)
        {
            Product? prod = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (prod == null)
            {
                return BadRequest(new
                {
                    error_message = "No product with such id exists"
                });
            }

            if (User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != prod.Seller.AuthId)
            {
                return BadRequest(new { error_message = "Not your product" });
            }

            prod.InStock = inStock;
            await resourceDbContext.SaveChangesAsync();
            return Ok(prod.Id);
        }

        /// <summary>
        /// Edit photos of a product, must be seller of that product or admin
        /// </summary>
        /// <param name="guid">Product ID</param>
        /// <param name="photos">Photos</param>
        /// <returns></returns>
        [HttpPatch("edit/{guid}/photos")]
        public async Task<IActionResult> EditPhotosAsync(string guid, [FromForm] IFormFile[] photos)
        {
            Product? prod = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (prod == null)
            {
                return BadRequest(new
                {
                    error_message = "No product with such id exists"
                });
            }

            if (User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != prod.Seller.AuthId)
            {
                return BadRequest(new { error_message = "Not your product" });
            }

            IEnumerable<string> references = await BlobWorker.AddPublicationPhotos(photos);
            if (!references.Any())
            {
                return BadRequest(new { error_message = "Photos has not been uploaded" });
            }
            
            string[] urls = prod.ProductPhotos.Select(p => p.Url).ToArray();
            await BlobWorker.RemovePublications(urls);

            List<ProductPhoto> productPhotos = new();
            foreach (string reference in references)
            {
                productPhotos.Add(new ProductPhoto()
                {
                    Url = reference
                });
            }

            prod.ProductPhotos = productPhotos;
            await resourceDbContext.SaveChangesAsync();
            return Ok(prod.Id);
        }

        /// <summary>
        /// Statistics of a product for seller's page
        /// </summary>
        /// <param name="guid">Product's id</param>
        /// <returns></returns>
        [HttpGet("statistics/{guid}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> StatisticsAsync(string guid)
        {
            Product? product = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (product == null)
            {
                return BadRequest(new
                {
                    error_message = "No product with such id exists"
                });
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != product.Seller.AuthId)
            {
                return BadRequest(new {error_message = "Not your product"});
            }

            Dictionary<int, int> SellingDict = new();

            for (int i = 0; i < 14; i++)
            {
                SellingDict[i] = resourceDbContext.Orders.Count(x => x.OrderTime >= DateTime.Today.AddDays(-i - 1) && x.OrderTime <= DateTime.Today.AddDays(-i));
            }

            return Ok(SellingDict);
        }

        /// <summary>
        /// Delete specific product, must be seller of that product or admin
        /// </summary>
        /// <param name="guid">Product's ID</param>
        /// <returns></returns>
        [HttpDelete("delete/{guid}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> DeleteAsync(string guid)
        {
            Product? product = await resourceDbContext.Products.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (product == null)
            {
                return BadRequest(new
                {
                    error_message = "No product with such id exists"
                });
            }

            if (User.IsInRole("Seller") && User.FindFirstValue(ClaimTypes.NameIdentifier) != product.Seller.AuthId)
            {
                return BadRequest(new {error_message = "Not your product"});
            }

            resourceDbContext.Products.Remove(product);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
