using Application.Interfaces.Business;
using Application.Interfaces.Database;
using Application.Interfaces.General;
using AutoMapper;
using Common.DTOs.Requests;
using Common.Services;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public class NotificationManager : INotificationManager
    {
        private readonly INotificationRepository _notificationDal;
        private readonly IMemberRepository _memberDal;
        private readonly IFileManager _fileManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContributionManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IEmailServiceCustom _emailService;
        private readonly ISMSService _smsService;

        public NotificationManager(
            INotificationRepository notificationDal,
            IFileManager fileManager,
            IUnitOfWork unitOfWork,
            ILogger<ContributionManager> logger,
            IMapper mapper,
            ICacheService cacheService,
            IEmailServiceCustom email,
            IMemberRepository memberDal,
            ISMSService smsService)
        {
            _notificationDal = notificationDal;
            _fileManager = fileManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
            _emailService = email;
            _memberDal = memberDal;
            _smsService = smsService;
        }

        /// <summary>
        /// Sends a notification based on the provided request.
        /// </summary>
        /// <param name="request">The notification request containing details to send the notification.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the notification was sent successfully, otherwise false.</returns>
        public async Task<bool> SendNotification(NotificationRequest request)
        {
            var notification = new Notification
            {
                MemberId = request.MemberId,
                Message = request.Message,
                NotificationReference = request.NotificationReference,
                Subject = request.Subject,
                NotificationType = request.NotificationType
            };

            try
            {
                // Send email notification
                await _emailService.SendEmailAsync(request.NotificationReference, request.Subject, request.Message);
                notification.IsSuccess = true;

                // Log success
                _logger.LogInformation("Notification sent successfully to {MemberId}", request.MemberId);
            }
            catch (Exception ex)
            {
                notification.IsSuccess = false;

                // Log error
                _logger.LogError(ex, "Failed to send notification to {MemberId}", request.MemberId);
            }

            // Save notification to the database
            await _notificationDal.Add(notification);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return notification.IsSuccess;
        }

        /// <summary>
        /// Sends a validation alert to the specified member.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the alert to.</param>
        /// <param name="message">The message to include in the alert.</param>
        public async Task SendValidationAlertAsync(string memberId, string message)
        {
            try
            {
                // Get member contact info
                var member = await _memberDal.Get(m => m.Id == memberId);
                if (member == null) return;

                await _emailService.SendEmailAsync(
                    toEmail: member.Email,
                    subject: "Contribution Validation Alert",
                    message: $"Dear {member.FirstName}, {message}");

                // Log success
                _logger.LogInformation("Validation alert sent successfully to {MemberId}", memberId);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send validation alert to {memberId}");
            }
        }

        /// <summary>
        /// Sends an eligibility notification to the specified member.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the notification to.</param>
        /// <param name="message">The message to include in the notification.</param>
        public async Task SendEligibilityNotificationAsync(string memberId, string message)
        {
            try
            {
                var member = await _memberDal.Get(m => m.Id == memberId);
                if (member == null) return;

                // Send both email and SMS
                await _emailService.SendEmailAsync(
                    toEmail: member.Email,
                    subject: "Pension Eligibility Update",
                    message: message);

                await _smsService.SendAsync(
                    phoneNumber: member.PhoneNumber,
                    message: message);

                // Log success
                _logger.LogInformation("Eligibility notification sent successfully to {MemberId}", memberId);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send eligibility notification to {memberId}");
            }
        }

        /// <summary>
        /// Sends an interest notification to the specified member.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the notification to.</param>
        /// <param name="message">The message to include in the notification.</param>
        public async Task SendInterestNotificationAsync(string memberId, string message)
        {
            try
            {
                var member = await _memberDal.Get(m => m.Id == memberId);
                if (member == null) return;

                await _emailService.SendEmailAsync(
                    toEmail: member.Email,
                    subject: "Interest Credited to Your Account",
                    message: message);

                // Log success
                _logger.LogInformation("Interest notification sent successfully to {MemberId}", memberId);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send interest notification to {memberId}");
            }
        }

        /// <summary>
        /// Sends a transaction alert to the specified member.
        /// </summary>
        /// <param name="memberId">The ID of the member to send the alert to.</param>
        /// <param name="message">The message to include in the alert.</param>
        public async Task SendTransactionAlertAsync(string memberId, string message)
        {
            try
            {
                var member = await _memberDal.Get(m => m.Id == memberId);
                if (member == null) return;

                await _smsService.SendAsync(
                    phoneNumber: member.PhoneNumber,
                    message: $"ALERT: {message}");

                // Log success
                _logger.LogInformation("Transaction alert sent successfully to {MemberId}", memberId);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send transaction alert to {memberId}");
            }
        }
    }
}
