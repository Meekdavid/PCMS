using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    /// <summary>
    /// Defines a custom email service interface for sending various types of emails.
    /// </summary>
    public interface IEmailServiceCustom
    {
        /// <summary>
        /// Sends an email asynchronously using an HTML file as the email body.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="htmlFilePath">The file path to the HTML content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string toEmail, string subject, string message);

        /// <summary>
        /// Sends a password reset token to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="callbackUrl">The URL to which the member should be redirected to reset their password.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPasswordResetToken(string email, string callbackUrl);

        /// <summary>
        /// Sends a confirmation email with a confirmation token to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="confirmationToken">The confirmation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendConfirmationEmail(string email, string confirmationToken);
    }
}
