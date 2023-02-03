using ECommerce1.Models;
using ECommerce1.Models.Validators;
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
    public class ReviewController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        public BlobWorker BlobWorker { get; set; }

        public ReviewController(ResourceDbContext resourceDbContext,
            BlobWorker blobWorker)
        {
            this.resourceDbContext = resourceDbContext;
            BlobWorker = blobWorker;
        }

        [HttpGet("product/{guid}")]
        public async Task<ActionResult<ReviewsViewModel>> GetByProductId(string guid, int page = 1, int onPage = 20)
        {
            var product = await resourceDbContext.Products
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Photos)
                .FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = product.Reviews
                .Skip((page - 1) * onPage)
                .Take(onPage)
                .Select(r => new ReviewReviewsModel
                {
                    Id = r.Id,
                    Quality = r.Quality,
                    ReviewText = r.ReviewText,
                    Photos = r.Photos.Select(p => p.Url).ToList(),
                    BuyerName = $"{r.User.FirstName[0]}. {r.User.LastName[0]}."
                }).ToList();

            return new ReviewsViewModel
            {
                Reviews = reviews,
                TotalProductCount = product.Reviews.Count,
                OnPageProductCount = onPage,
                CurrentPage = page,
                TotalPageCount = (int)Math.Ceiling((double)product.Reviews.Count / onPage)
            };
        }

        [HttpPost("add")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddReview([FromForm] AddReviewViewModel review)
        {
            string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == id);
            if (user == null)
            {
                return BadRequest("No such user exists");
            }
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(c => c.Id.ToString() == review.ProductId);
            if(product == null)
            {
                return BadRequest("No such product exists");
            }
            Review? review1 = await resourceDbContext.Reviews.FirstOrDefaultAsync(r => r.User.AuthId == id);
            if(review1 != null)
            {
                return BadRequest("You have already submitted a review for this product");
            }

            IEnumerable<string> references = await BlobWorker.AddPublicationPhotos(review.Photos);
            List<ReviewPhoto> productPhotos = new();
            foreach (string reference in references)
            {
                productPhotos.Add(new ReviewPhoto()
                {
                    Url = reference
                });
            }

            Review rev = new()
            {
                User = user,
                Product = product,
                ReviewText = review.Text,
                Quality = review.Rating,
                Photos = productPhotos
            };

            await resourceDbContext.Reviews.AddAsync(rev);
            await resourceDbContext.SaveChangesAsync();
            return Ok(rev.Id);
        }

        [HttpDelete("delete")]
        [Authorize("User,Admin")]
        public async Task<IActionResult> DeleteAsync(string guid)
        {
            Review? review = await resourceDbContext.Reviews.Include(p => p.User).FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if (review == null)
            {
                return BadRequest("No review with such id exists");
            }
            if (User.IsInRole("User") && User.FindFirstValue(ClaimTypes.NameIdentifier) != review.User.AuthId)
            {
                return BadRequest();
            }

            resourceDbContext.Reviews.Remove(review);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
