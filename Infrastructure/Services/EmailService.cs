using Application.Interfaces.General;
using Common.ConfigurationSettings;
using Common.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;

//using System.Net.Mail;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EmailService : IEmailServiceCustom
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer = ConfigSettings.ApplicationSetting.EmailDetails.SMTPServer;
        private readonly int _smtpPort = ConfigSettings.ApplicationSetting.EmailDetails.Port; // Use 465 for SSL, 587 for TLS
        private readonly string _smtpMember = AESHelper.Decrypt(ConfigSettings.ApplicationSetting.EmailDetails.MemberName);
        private readonly string _smtpPass = AESHelper.Decrypt(ConfigSettings.ApplicationSetting.EmailDetails.Password);
        private readonly SmtpClient _smtpClient;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpClient = new SmtpClient();
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("NLPC Pension", _smtpMember));
            email.To.Add(new MailboxAddress(toEmail, toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            email.Body = bodyBuilder.ToMessageBody();

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await _smtpClient.AuthenticateAsync(_smtpMember, _smtpPass);
            }
            await _smtpClient.SendAsync(email);
        }

        public async Task SendPasswordResetToken(string email, string callbackUrl)
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("NLPC Pension", _smtpMember));
            mailMessage.To.Add(new MailboxAddress(email, email));
            mailMessage.Subject = "Password Reset";
            mailMessage.Body = new TextPart("html")
            {
                Text = $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>."
            };

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await _smtpClient.AuthenticateAsync(_smtpMember, _smtpPass);
            }

            await _smtpClient.SendAsync(mailMessage);
        }

        public void Dispose()
        {
            if (_smtpClient.IsConnected)
            {
                _smtpClient.Disconnect(true);
            }
            _smtpClient.Dispose();
        }

        public async Task SendConfirmationEmail(string email, string confirmationToken)
        {
            // Encode the token and email for use in the URL
            string encodedToken = Uri.EscapeDataString(confirmationToken);
            string encodedEmail = Uri.EscapeDataString(email);

            // Construct the confirmation link
            string confirmationLink = $"{ConfigSettings.ApplicationSetting.BaseLocalStorageDomain}api/auth/confirm-email?token={encodedToken}&email={encodedEmail}";

            // Create the MimeMessage
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("NLPC Pension", _smtpMember));
            mailMessage.To.Add(new MailboxAddress(email, email));
            mailMessage.Subject = "Confirm your email";

            // Create the HTML body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = ConfigSettings.ApplicationSetting.EmailDetails.WelcomeEmail.Replace("{confirmationLink}", confirmationLink)
            };

            mailMessage.Body = bodyBuilder.ToMessageBody();

            // Ensure the SMTP client is connected
            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await _smtpClient.AuthenticateAsync(_smtpMember, _smtpPass);
            }

            // Send the email
            await _smtpClient.SendAsync(mailMessage);
        }
    }
}