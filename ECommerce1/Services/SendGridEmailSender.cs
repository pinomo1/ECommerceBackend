using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ECommerce1.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly ISendGridClient sendGridClient;
        private readonly ILogger logger;

        public SendGridEmailSender(ISendGridClient sendGridClient, ILogger<SendGridEmailSender> logger)
        {
            this.sendGridClient = sendGridClient;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("sams_bd16@itstep.edu.az", "ECommerce App"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            var response = await sendGridClient.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Email queued successfully");
            }
            else
            {
                logger.LogError("Failed to send email");
            }
        }
    }
}
