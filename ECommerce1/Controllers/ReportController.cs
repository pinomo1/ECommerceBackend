using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        public readonly IEmailSender emailSender;

        public ReportController(ResourceDbContext resourceDbContext, IEmailSender _emailSender)
        {
            this.resourceDbContext = resourceDbContext;
            this.emailSender = _emailSender;
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportSomething(string? ReporterEmail, string? ReporterName, ReportType ReportType, string? ReportedItemId, string? ReportText)
        {
            bool isAuthorized;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ReporterEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                ReporterName = User.FindFirst(ClaimTypes.Name)?.Value;
                isAuthorized = true;
            }
            else
            {
                ReporterEmail = ReporterEmail?.Trim();
                ReporterName = ReporterName?.Trim();
                isAuthorized = false;
            }
            if ((ReportType == ReportType.Other || ReportType == ReportType.Suggestion) && string.IsNullOrWhiteSpace(ReportText))
            {
                return BadRequest(new
                {
                    error_message = "Complaint text is required"
                });
            }
            if (ReportType == ReportType.Suggestion && !string.IsNullOrWhiteSpace(ReportedItemId))
            {
                return BadRequest(new
                {
                    error_message = "Suggestion can't have reported item id"
                });
            }
            Report report = new()
            {
                ReporterEmail = ReporterEmail,
                ReporterName = ReporterName,
                ReportType = ReportType,
                ReportedItemId = ReportedItemId,
                ReportStatus = ReportStatus.Pending,
                ReportText = ReportText,
                IsAuthorized = isAuthorized
            };
            await resourceDbContext.Reports.AddAsync(report);
            await resourceDbContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Reported successfully"
            });
        }

        [HttpGet("get_unresolved_admin")]
        public async Task<ActionResult<IList<Report>>> GetUnresolvedAdmin(int page = 1)
        {
            return await resourceDbContext.Reports.Where(r => r.ReportStatus == ReportStatus.Pending).Skip((page - 1) * 20).Take(20).ToListAsync();
        }

        [HttpGet("get_by_user")]
        [Authorize]
        public async Task<ActionResult<IList<Report>>> GetByUser(bool onlyUnresolved, int page = 1)
        {
            string? authId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (authId == null) {
                return BadRequest(new
                {
                    error_message = "User not found"
                });
            }
            return await resourceDbContext.Reports.Where(r => r.ReporterName == authId && r.IsAuthorized && (!onlyUnresolved || r.ReportStatus == ReportStatus.Pending)).Skip((page - 1) * 20).Take(20).ToListAsync();
        }

        [HttpPatch("resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(string id, ReportStatus reportStatus)
        {
            Report? report = await resourceDbContext.Reports.FindAsync(id);
            if (report == null)
            {
                return BadRequest(new
                {
                    error_message = "Report not found"
                });
            }
            if (reportStatus == ReportStatus.Pending)
            {
                return BadRequest(new
                {
                    error_message = "Report status can't be pending"
                });
            }
            report.ReportStatus = reportStatus;
            await resourceDbContext.SaveChangesAsync();
            await emailSender.SendEmailAsync(report.ReporterEmail, "Report update",
                $"Your report with id {report.Id} has been resolved. The status is now {(reportStatus == ReportStatus.Resolved ? "resolved" : "rejected")}");
            return Ok(new
            {
                message = "Report resolved successfully"
            });
        }
    }
}
