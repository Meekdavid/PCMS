using Common.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Business
{
    /// <summary>
    /// Interface for managing notifications.
    /// </summary>
    public interface INotificationManager
    {
        /// <summary>
        /// Sends a notification based on the provided request.
        /// </summary>
        /// <param name="request">The notification request containing details of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> SendNotification(NotificationRequest request);

        /// <summary>
        /// Sends a validation alert asynchronously.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the alert to.</param>
        /// <param name="message">The message to be sent in the alert.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendValidationAlertAsync(string memberId, string message);

        /// <summary>
        /// Sends an eligibility notification asynchronously.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the notification to.</param>
        /// <param name="message">The message to be sent in the notification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendEligibilityNotificationAsync(string memberId, string message);

        /// <summary>
        /// Sends an interest notification asynchronously.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the notification to.</param>
        /// <param name="message">The message to be sent in the notification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendInterestNotificationAsync(string memberId, string message);

        /// <summary>
        /// Sends a transaction alert asynchronously.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the alert to.</param>
        /// <param name="message">The message to be sent in the alert.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendTransactionAlertAsync(string memberId, string message);
    }
}
