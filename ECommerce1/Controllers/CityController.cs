using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public CityController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }

        /// <summary>
        /// Get all cities
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<IEnumerable<City>> GetAsync()
        {
            return await resourceDbContext.Cities.ToListAsync();
        }

        /// <summary>
        /// Get all cities by country id
        /// </summary>
        /// <param name="id">Country's id</param>
        /// <returns></returns>
        [HttpGet("country/{id}")]
        public async Task<ActionResult<IEnumerable<City>>> GetByCountryAsync(string id)
        {
            Country? country = resourceDbContext.Countries.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == id.ToLower().Trim());
            if(country == null)
            {
                return BadRequest("Country doesn't exists");
            }
            return await resourceDbContext.Cities.Include(c => c.Country).Where(c => c.Country.Id.ToString().ToLower().Trim() == id.ToLower().Trim()).ToListAsync();
        }


        /// <summary>
        /// Add city as admin given country id
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsync(string countryId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Bad name");
            }
            if(resourceDbContext.Cities.FirstOrDefault(c => c.Name.ToLower().Trim() == name.ToLower().Trim() && c.Country.Id.ToString() == countryId) != null)
            {
                return BadRequest("City already exists");
            }
            Country? country = resourceDbContext.Countries.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == countryId.ToLower().Trim());
            if (country == null)
            {
                return BadRequest("Country doesn't exists");
            }
            City city = new()
            {
                Name = name,
                Country = country
            };
            await resourceDbContext.Cities.AddAsync(city);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Rename city as Admin
        /// </summary>
        /// <param name="id">City's id</param>
        /// <param name="name">New name</param>
        /// <returns></returns>
        [HttpPatch("rename")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RenameAsync(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Bad name");
            }
            City? city = resourceDbContext.Cities.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == id.ToLower().Trim());
            if (city == null)
            {
                return BadRequest("City doesn't exists");
            }
            city.Name = name;
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Delete city by id as Admin
        /// </summary>
        /// <param name="id">City's id</param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            City? city = resourceDbContext.Cities.FirstOrDefault(c => c.Id.ToString().ToLower().Trim() == id.ToLower().Trim());
            if(city == null)
            {
                return BadRequest("No such city");
            }
            resourceDbContext.Cities.Remove(city);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
