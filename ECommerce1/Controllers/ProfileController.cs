using Azure.Storage.Blobs;
using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("returnMyInfo")]
        [Authorize(Roles ="User")]
        public async Task<IActionResult> ReturnMyInfo()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
            {
                return BadRequest(new { error_message = "Not logged in" });
            }
            return Ok(user);
        }
    }
}
