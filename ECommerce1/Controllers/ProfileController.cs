using Azure.Storage.Blobs;
using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;

        public ProfileController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        /// <summary>
        /// Return any authorized user's info
        /// </summary>
        /// <returns></returns>
        [HttpGet("returnMyInfo")]
        [Authorize()]
        public async Task<IActionResult> ReturnMyInfo()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            AUser? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
            {
                user = await resourceDbContext.Sellers.FirstOrDefaultAsync(u => u.AuthId == userId);
                if(user == null)
                {
                    user = await resourceDbContext.Staffs.FirstOrDefaultAsync(u => u.AuthId == userId);
                    if(user == null)
                    {
                       return BadRequest(new { error_message = "Not logged in" });                        
                    }
                }
            }
            return Ok(user);
        }

        [HttpPatch("changeMyInfo")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangeMyInfo(string first, string last)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
                return BadRequest(new
                {
                    error_message = "User not found"
                });
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last))
                return BadRequest(new
                {
                    error_message = "First and last name cannot be empty"
                });
            user.FirstName = first;
            user.LastName = last;
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
