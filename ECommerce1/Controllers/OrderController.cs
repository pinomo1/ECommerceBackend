using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static ECommerce1.Controllers.ProductController;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public OrderController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }
        

        [HttpGet("states")]
        public async Task<ActionResult<IList<string>>> GetStatesEnum()
        {
            IDictionary<int, string> names = Enum.GetNames(typeof(OrderStatus)).ToList().Select((s, i) => new { s, i }).ToDictionary(x => x.i + 1, x => x.s);

            List<TempStruct111> tsList = new();

            foreach (var item in names)
            {
                tsList.Add(new() { Key = item.Key, Value = item.Value });
            }

            return Ok(tsList);
        }

        [HttpGet("getOwn")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IList<Order>>> GetOrders(int page = 1)
        {
            if (page < 1)
            {
                return BadRequest(new { error_message = "Page must be > 1" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Order> cartItems = await resourceDbContext.Orders.Where(ci => ci.User.AuthId == userId).Include(ci => ci.Product).OrderByDescending(o => o.OrderTime).Skip((page-1) * 20).Take(20).ToListAsync();
            return Ok(cartItems);
        }

        [HttpGet("getOwnSeller")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<IList<Order>>> GetOrdersSeller(int page = 1)
        {
            if(page < 1)
            {
                return BadRequest(new { error_message = "Page must be > 1" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Order> cartItems = await resourceDbContext.Orders.Where(ci => ci.Product.Seller.AuthId == userId).Include(ci => ci.Product).OrderByDescending(o => o.OrderTime).Skip((page-1)*20).Take(20).ToListAsync();
            return Ok(cartItems);
        }

        [HttpPost("addNow/{guid}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddOrderNow(string guid, string addressGuid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if(profile == null)
            {
                return BadRequest(new { error_message = "No such profile was found" });
            }
            Address? address = await resourceDbContext.Addresses.Include(a => a.City).ThenInclude(c => c.Country).FirstOrDefaultAsync(a => a.User.AuthId == userId && a.Id.ToString() == addressGuid);
            if(address == null)
            {
                return BadRequest(new { error_message = "No such address was found" });
            }
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest(new { error_message = "No such product was found" });
            }
            Order order = new()
            {
                AddressCopy = address.Normalize(profile.PhoneNumber),
                OrderTime = DateTime.Now,
                Product = product,
                User = profile
            };

            await resourceDbContext.Orders.AddAsync(order);
            await resourceDbContext.SaveChangesAsync();

            return Ok(order.Id);
        }

        [HttpPost("addFromCart")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddFromCart(string addressGuid)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == userId);
            if (profile == null)
            {
                return BadRequest(new { error_message = "No such profile was found" });
            }
            Address? address = await resourceDbContext.Addresses.Include(a => a.City).ThenInclude(c => c.Country).FirstOrDefaultAsync(a => a.User.AuthId == userId && a.Id.ToString() == addressGuid);
            if (address == null)
            {
                return BadRequest(new { error_message = "No such address was found" });
            }
            IList<CartItem>? products = await resourceDbContext.CartItems.Include(ci => ci.Product).Where(p => p.User.AuthId.ToString() == userId).ToListAsync();
            if (products == null)
            {
                return BadRequest(new { error_message = "No product were found" });
            }
            List<Order> orders = new();

            foreach (var item in products)
            {
                orders.Add(new()
                {
                    AddressCopy = address.Normalize(profile.PhoneNumber),
                    OrderTime = DateTime.Now,
                    Product = item.Product,
                    User = profile
                });
            }

            await resourceDbContext.Orders.AddRangeAsync(orders);
            resourceDbContext.CartItems.RemoveRange(products);
            await resourceDbContext.SaveChangesAsync();

            return Ok(orders.Select(o => new { id = o.Id }));
        }

        [HttpPatch("changeStatus")]
        [Authorize(Roles = "User,Seller")]
        public async Task<IActionResult> ChangeStatus(string orderGuid, int status)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Order? order = await resourceDbContext.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id.ToString() == orderGuid);
            if (order == null)
            {
                return BadRequest(new { error_message = "No such order was found" });
            }
            if (order.Product.Seller.AuthId != userId && order.User.AuthId != userId)
            {
                return BadRequest(new { error_message = "You are not allowed to change this order" });
            }
            if (status < 1 || status > 5)
            {
                return BadRequest(new { error_message = "No such status was found" });
            }
            OrderStatus orderStatus = (OrderStatus)status;
            if (order.OrderStatus == (int)OrderStatus.Returned || order.OrderStatus == (int)OrderStatus.Cancelled || order.OrderStatus == (int)OrderStatus.Delivered)
            {
                return BadRequest(new { error_message = "You are not allowed to change this order" });
            }
            if (order.Product.Seller.AuthId == userId)
            {
                if(order.OrderStatus == (int)OrderStatus.Unverified)
                {
                    if(orderStatus == OrderStatus.Cancelled || orderStatus == OrderStatus.Delivering)
                    {
                        order.OrderStatus = status;
                    }
                    else
                    {
                        return BadRequest(new { error_message = "Change unverified order to cancelled or delivering" });
                    }
                }
                else if(order.OrderStatus == (int)OrderStatus.Cancelling)
                {
                    if(orderStatus == OrderStatus.Cancelled)
                    {
                        order.OrderStatus = status;
                    }
                    else
                    {
                        return BadRequest(new { error_message = "Change cancelling order to cancelled" });
                    }
                }
                else if(order.OrderStatus == (int)OrderStatus.Returning)
                {
                    if (orderStatus == OrderStatus.Returned)
                    {
                        order.OrderStatus = status;
                    }
                    else
                    {
                        return BadRequest(new { error_message = "Change returning order to returned" });
                    }
                }
                else if(order.OrderStatus == (int)OrderStatus.Delivering)
                {
                    if (orderStatus == OrderStatus.Delivered)
                    {
                        order.OrderStatus = status;
                    }
                    else
                    {
                        return BadRequest(new { error_message = "Change returning order to returned" });
                    }
                }
                else
                {
                    return BadRequest(new { error_message = "You can't do that" });
                }
            }
            else
            {
                if(order.OrderStatus == (int)OrderStatus.Delivering)
                {
                    if(orderStatus == OrderStatus.Returning || orderStatus == OrderStatus.Cancelling)
                    {
                        order.OrderStatus = status;
                    }
                    else
                    {
                        return BadRequest(new { error_message = "Change delivering order to returning or cancelling" });
                    }
                }
            }
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
