using Azure.Storage.Blobs;
using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public BlobWorker BlobWorker { get; set; }

        public SellerController(ResourceDbContext resourceDbContext, IConfiguration configuration, BlobWorker blobWorker)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
            BlobWorker = blobWorker;
        }

        [HttpGet("title/{title}")]
        public async Task<ActionResult<IList<Seller>>> ByTitle(string title)
        {
            if(title.Length < 4)
            {
                return BadRequest("Name is too shot");
            }
            IList<Seller> sellers = await resourceDbContext.Sellers
                .Where(s => EF.Functions.Like(s.CompanyName, $"%{title}%")).ToListAsync();
            return Ok(sellers);
        }

        [HttpPost("postpfp")]
        [Authorize("Seller")]
        public async Task<IActionResult> PostProfilePicture(IFormFile? picture)
        {
            Seller? seller = await resourceDbContext.Sellers.FirstOrDefaultAsync(s => s.AuthId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if(seller == null)
            {
                return BadRequest("Not authorized");
            }
            string reference = await BlobWorker.AddPublicationPhoto(picture);
            if(reference == String.Empty)
            {
                return BadRequest("Bad photo");
            }
            seller.ProfilePhotoUrl = reference;
            await resourceDbContext.SaveChangesAsync();
            return Ok(seller.Id);
        }
    }
}
