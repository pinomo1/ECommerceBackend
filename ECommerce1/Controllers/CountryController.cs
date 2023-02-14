using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public CountryController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }

        /// <summary>
        /// Get all countries
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<IEnumerable<Country>> GetAsync()
        {
            return await resourceDbContext.Countries.ToListAsync();
        }

        /// <summary>
        /// Add new country as admin
        /// </summary>
        /// <param name="name">Country's name</param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    error_message = "Bad name"
                });
            }
            if (resourceDbContext.Countries.FirstOrDefault(c => c.Name.ToLower().Trim() == name.ToLower().Trim()) != null)
            {
                return BadRequest(new
                {
                    error_message = "Country already exists"
                });
            }
            Country country = new() { Name = name };
            await resourceDbContext.Countries.AddAsync(country);
            await resourceDbContext.SaveChangesAsync();
            return Ok(country.Id);
        }


        /// <summary>
        /// Rename country as admin
        /// </summary>
        /// <param name="id">Country's id</param>
        /// <param name="name">New name</param>
        /// <returns></returns>
        [HttpPatch("rename")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RenameAsync(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    error_message = "Bad name"
                });
            }
            Country? country = resourceDbContext.Countries.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == id.ToLower().Trim());
            if (country == null)
            {
                return BadRequest(new
                {
                    error_message = "Country doesn't exists"
                });
            }
            country.Name = name;
            await resourceDbContext.SaveChangesAsync();
            return Ok(country.Id);
        }


        /// <summary>
        /// Delete country as admin
        /// </summary>
        /// <param name="id">Country's id</param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            Country? country = resourceDbContext.Countries.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == id.ToLower().Trim());
            if (country == null)
            {
                return BadRequest(new
                {
                    error_message = "No such country"
                });
            }
            resourceDbContext.Countries.Remove(country);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
