﻿using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController(ResourceDbContext resourceDbContext, IConfiguration configuration) : ControllerBase
    {


        /// <summary>
        /// Get all your own addresses (as a user)
        /// </summary>
        /// <returns></returns>
        [HttpGet("get_own")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<IList<AddressViewModel>>> GetAddresses()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<AddressViewModel> addresses = await resourceDbContext.WarehouseAddresses.Where(a => a.User.AuthId == userId).Select(a => new AddressViewModel()
            {
                Id = a.Id.ToString(),
                First = a.First,
                Second = a.Second ?? "",
                Zip = a.Zip,
                City = new() { Id = a.City.Id.ToString(), Name = a.City.Name },
                Country = new() { Id = a.City.Country.Id.ToString(), Name = a.City.Country.Name }
            }).ToListAsync();
            return Ok(addresses);
        }

        /// <summary>
        /// Add address to address list
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddAddress(AddAddressViewModel address)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Seller? user = await resourceDbContext.Sellers.FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
                return BadRequest(new
                {
                    error_message = "User not found"
                });
            City? city = await resourceDbContext.Cities.FirstOrDefaultAsync(c => c.Id.ToString() == address.CityId);
            if (city == null)
                return BadRequest(new
                {
                    error_message = "City not found"
                });

            WarehouseAddress newAddress = new()
            {
                City = city,
                User = user,
                First = address.First,
                Second = address.Second,
                Zip = address.Zip
            };
            await resourceDbContext.WarehouseAddresses.AddAsync(newAddress);
            await resourceDbContext.SaveChangesAsync();
            return Ok(newAddress.Id);
        }

        /// <summary>
        /// Delete address from address list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteAddress(string id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WarehouseAddress? address = await resourceDbContext.WarehouseAddresses.Include(a => a.User).FirstOrDefaultAsync(a => a.Id.ToString() == id);
            if (address == null)
                return BadRequest(new
                {
                    error_message = "UserAddress not found"
                });
            if (address.User.AuthId != userId)
                return BadRequest(new
                {
                    error_message = "You are not authorized to delete this address"
                });
            resourceDbContext.WarehouseAddresses.Remove(address);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Edit existing address
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPut("edit")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> EditAddress(string addressId, AddAddressViewModel address)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WarehouseAddress? oldAddress = await resourceDbContext.WarehouseAddresses.Include(a => a.User).FirstOrDefaultAsync(a => a.Id.ToString() == addressId);
            if (oldAddress == null)
                return BadRequest(new
                {
                    error_message = "UserAddress not found"
                });
            if (oldAddress.User.AuthId != userId)
                return BadRequest(new
                {
                    error_message = "You are not authorized to edit this address"
                });
            City? city = await resourceDbContext.Cities.FirstOrDefaultAsync(c => c.Id.ToString() == address.CityId);
            if (city == null)
                return BadRequest(new
                {
                    error_message = "City not found"
                });

            oldAddress.First = address.First;
            oldAddress.Second = address.Second;
            oldAddress.City = city;
            oldAddress.Zip = address.Zip;
            await resourceDbContext.SaveChangesAsync();
            return Ok(oldAddress.Id);
        }
    }
}
