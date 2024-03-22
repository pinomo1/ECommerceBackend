using ECommerce1.Models;
using ECommerce1.Models.Validators;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionProductController(ResourceDbContext resourceDbContext,
        BlobWorker blobWorker) : ControllerBase
    {

        /// <summary>
        /// Get all questions for specified product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>List of questions</returns>
        [HttpGet("product/{id}")]
        public async Task<ActionResult<IList<QuestionProduct>>> GetQAByProduct(string id)
        {
            if (!await resourceDbContext.Products.AnyAsync(p => p.Id.ToString() == id)) {
                return NotFound(new { error_message = "No such product exists" });
            }

            List<QuestionProduct> questions = await resourceDbContext.QuestionProducts
                .Where(q => q.Product.Id.ToString() == id)
                .ToListAsync();

            return Ok(questions);
        }

        /// <summary>
        /// Delete question by id
        /// </summary>
        /// <param name="id">Id of question</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller,User")]
        public async Task<ActionResult> DeleteQuestion(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin"))
            {
                if(User.IsInRole("User"))
                {
                    var user = await resourceDbContext.Profiles
                        .FirstOrDefaultAsync(u => u.AuthId == userId);
                    if (user == null)
                    {
                          return NotFound(new { error_message = "No such user exists" });
                    }
                }
                else if (User.IsInRole("Seller"))
                {
                    var seller = await resourceDbContext.Sellers
                        .FirstOrDefaultAsync(s => s.AuthId == userId);
                    if (seller == null)
                    {
                        return NotFound(new { error_message = "No such seller exists" });
                    }
                }
                else
                {
                    return BadRequest(new { error_message = "No such role exists" });
                }
            }

            var question = await resourceDbContext.QuestionProducts
                .FirstOrDefaultAsync(q => q.Id.ToString() == id);

            if (question == null)
            {
                return NotFound(new { error_message = "No such question exists" });
            }

            resourceDbContext.QuestionProducts.Remove(question);
            await resourceDbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Add question to product
        /// </summary>
        /// <param name="id">Id of product</param>
        /// <param name="question">Question</param>
        /// <returns></returns>
        [HttpPost("add/question")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> AddQuestion(string id, string question)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await resourceDbContext.Profiles
                .FirstOrDefaultAsync(u => u.AuthId == userId);
            if (user == null)
            {
                return NotFound(new { error_message = "No such user exists" });
            }

            var product = await resourceDbContext.Products
                .FirstOrDefaultAsync(p => p.Id.ToString() == id);
            if (product == null)
            {
                return NotFound(new { error_message = "No such product exists" });
            }

            var questionProduct = new QuestionProduct()
            {
                Product = product,
                User = user,
                Question = question
            };

            await resourceDbContext.QuestionProducts.AddAsync(questionProduct);
            await resourceDbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Answer question
        /// </summary>
        /// <param name="id">Id of question</param>
        /// <param name="answer">Answer</param>
        /// <returns></returns>
        [HttpPost("add/answer")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult> AddAnswer(string id, string answer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var seller = await resourceDbContext.Sellers
                .FirstOrDefaultAsync(s => s.AuthId == userId);
            if (seller == null)
            {
                return NotFound(new { error_message = "No such seller exists" });
            }

            var question = await resourceDbContext.QuestionProducts
                .FirstOrDefaultAsync(q => q.Id.ToString() == id);
            if (question == null)
            {
                return NotFound(new { error_message = "No such question exists" });
            }

            question.Answer = answer;
            await resourceDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
