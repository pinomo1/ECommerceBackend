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
    public class AddressController : Controller
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;
        public AddressController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        [HttpGet("get_own")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAddresses()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Address> addresses = await resourceDbContext.Addresses.Where(a => a.User.AuthId == userId).ToListAsync();
            return Ok(addresses);
        }

        [HttpPost("add")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddAddress(Address address)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
                return BadRequest("User not found");
            address.User = user;
            await resourceDbContext.Addresses.AddAsync(address);
            await resourceDbContext.SaveChangesAsync();
            return Ok(address);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteAddress(string id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Address? address = await resourceDbContext.Addresses.FirstOrDefaultAsync(a => a.Id.ToString() == id);
            if (address == null)
                return BadRequest("Address not found");
            if (address.User.AuthId != userId)
                return BadRequest("You are not authorized to delete this address");
            resourceDbContext.Addresses.Remove(address);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("edit")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditAddress(Address address)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Address? oldAddress = await resourceDbContext.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id);
            if (oldAddress == null)
                return BadRequest("Address not found");
            if (oldAddress.User.AuthId != userId)
                return BadRequest("You are not authorized to edit this address");
            oldAddress.First = address.First;
            oldAddress.Second = address.Second;
            oldAddress.City = address.City;
            oldAddress.Zip = address.Zip;
            await resourceDbContext.SaveChangesAsync();
            return Ok(oldAddress);
        }
    }
}
