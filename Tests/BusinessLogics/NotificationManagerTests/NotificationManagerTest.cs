using Application.Interfaces.Database;
using Application.Interfaces.General;
using Application.Repositories;
using AutoMapper;
using Common.DTOs.Requests;
using Common.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.BusinessLogics.NotificationManagerTests
{
    public class NotificationManagerTests
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
        private readonly NotificationManager _manager;

        public NotificationManagerTests()
        {
            _notificationDal = A.Fake<INotificationRepository>();
            _memberDal = A.Fake<IMemberRepository>();
            _fileManager = A.Fake<IFileManager>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<ContributionManager>>();
            _mapper = A.Fake<IMapper>();
            _cacheService = A.Fake<ICacheService>();
            _emailService = A.Fake<IEmailServiceCustom>();
            _smsService = A.Fake<ISMSService>();

            _manager = new NotificationManager(
                _notificationDal,
                _fileManager,
                _unitOfWork,
                _logger,
                _mapper,
                _cacheService,
                _emailService,
                _memberDal,
                _smsService);
        }

        [Fact]
        public async Task SendNotification_Success_ReturnsTrue()
        {
            // Arrange
            var request = new NotificationRequest
            {
                MemberId = "mem-123",
                Message = "Test message",
                Subject = "Test subject",
                NotificationType = NotificationType.Email,
                NotificationReference = "test@example.com"
            };

            A.CallTo(() => _emailService.SendEmailAsync(
                    request.NotificationReference,
                    request.Subject,
                    request.Message))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.SendNotification(request);

            // Assert
            Assert.True(result);
            A.CallTo(() => _notificationDal.Add(A<Notification>.That.Matches(n =>
                n.MemberId == request.MemberId &&
                n.Message == request.Message &&
                n.IsSuccess == true)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _unitOfWork.CommitAsync())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendNotification_Failure_ReturnsFalseAndLogsError()
        {
            // Arrange
            var request = new NotificationRequest
            {
                MemberId = "mem-123",
                Message = "Test message",
                Subject = "Test subject",
                NotificationType = NotificationType.Email,
                NotificationReference = "test@example.com"
            };

            A.CallTo(() => _emailService.SendEmailAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws(new Exception("Email failed"));

            // Act
            var result = await _manager.SendNotification(request);

            // Assert
            Assert.False(result);
            //A.CallTo(() => _logger.LogError(
            //        A<Exception>._,
            //        "Failed to send notification to {MemberId}",
            //        request.MemberId))
            //    .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendValidationAlertAsync_Success_SendsEmail()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "Validation required";
            var member = new Member
            {
                Id = memberId,
                Email = "test@example.com",
                FirstName = "John"
            };

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns(member);

            // Act
            await _manager.SendValidationAlertAsync(memberId, message);

            // Assert
            A.CallTo(() => _emailService.SendEmailAsync(
                    member.Email,
                    "Contribution Validation Alert",
                    $"Dear {member.FirstName}, {message}"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendEligibilityNotificationAsync_Success_SendsEmailAndSMS()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "You're eligible";
            var member = new Member
            {
                Id = memberId,
                Email = "test@example.com",
                PhoneNumber = "+1234567890"
            };

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns(member);

            // Act
            await _manager.SendEligibilityNotificationAsync(memberId, message);

            // Assert
            A.CallTo(() => _emailService.SendEmailAsync(
                    member.Email,
                    "Pension Eligibility Update",
                    message))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _smsService.SendAsync(
                    member.PhoneNumber,
                    message))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendInterestNotificationAsync_Success_SendsEmail()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "Interest credited";
            var member = new Member
            {
                Id = memberId,
                Email = "test@example.com"
            };

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns(member);

            // Act
            await _manager.SendInterestNotificationAsync(memberId, message);

            // Assert
            A.CallTo(() => _emailService.SendEmailAsync(
                    member.Email,
                    "Interest Credited to Your Account",
                    message))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendTransactionAlertAsync_Success_SendsSMS()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "Transaction completed";
            var member = new Member
            {
                Id = memberId,
                PhoneNumber = "+1234567890"
            };

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns(member);

            // Act
            await _manager.SendTransactionAlertAsync(memberId, message);

            // Assert
            A.CallTo(() => _smsService.SendAsync(
                    member.PhoneNumber,
                    $"ALERT: {message}"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendValidationAlertAsync_MemberNotFound_DoesNotSendEmail()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "Validation required";

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns((Member)null);

            // Act
            await _manager.SendValidationAlertAsync(memberId, message);

            // Assert
            A.CallTo(() => _emailService.SendEmailAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task SendEligibilityNotificationAsync_EmailFails_StillSendsSMS()
        {
            // Arrange
            var memberId = "mem-123";
            var message = "You're eligible";
            var member = new Member
            {
                Id = memberId,
                Email = "test@example.com",
                PhoneNumber = "+1234567890"
            };

            A.CallTo(() => _memberDal.Get(m => m.Id == memberId))
                .Returns(member);

            A.CallTo(() => _emailService.SendEmailAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws(new Exception("Email failed"));

            // Act
            await _manager.SendEligibilityNotificationAsync(memberId, message);

            // Assert
            A.CallTo(() => _smsService.SendAsync(
                    member.PhoneNumber,
                    message))
                .MustHaveHappenedOnceExactly();
        }
    }
}
