using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Builder.Extensions;
using IdentityService.Options;
using Microsoft.Extensions.Options;

namespace IdentityService.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly EmailSenderOptions _emailSenderOptions;
        public EmailSender(IOptions<EmailSenderOptions> options)
        {
            _emailSenderOptions = options.Value;
        }
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            await SendEmailAsync(email, "Confirmation Link", confirmationLink);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            await SendEmailAsync(email, "Password Reset Code", resetCode);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            await SendEmailAsync(email, "Password Reset Link", resetLink);
        }

        private async Task SendEmailAsync(string email, string subject, string messageBody)
        {
            using var client = new SmtpClient(_emailSenderOptions.Host, _emailSenderOptions.Port)
            {
                Credentials = new NetworkCredential(_emailSenderOptions.Username, _emailSenderOptions.Password),
                EnableSsl = _emailSenderOptions.UseSsl
            };

            var message = new MailMessage(_emailSenderOptions.FromEmail, email)
            {
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = true // Set to true if the body contains HTML
            };

            await client.SendMailAsync(message);
        }
    }
}
