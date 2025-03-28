using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Enums
{
    public enum Status
    {
        Passive = 1, Active, Deleted
    }

    public enum ContributionType
    {
        Monthly = 1,
        Voluntary
    }

    public enum BackgroundJobType
    {
        ValidateContributions = 1,
        GenerateEligibilityUpdates,
        HandleFailedTransactions
    }

    public enum JobStatus
    {
        Pending = 1,
        Completed,
        Failed
    }

    public enum MembershipType
    {
        Employer = 1,
        Employee,
        Individual,
    }

    public enum AccountType
    {
        IndividualContribution =1,      // VCA
        EmployerSponsoredPension,   // Employer Pension Scheme
    }

    public enum StatementFormat
    {
        PDF =1,
        Excel
    }

    public enum TransactionType
    {
        Contribution = 1, // Member making a pension contribution
        Withdrawal,   // Member withdrawing pension funds
        Interest,     // Transferring pension funds to another account
        Refund        // Refund of overpaid contributions
    }

    public enum TransactionStatus
    {
        Pending = 1,
        Completed,
        Failed,
        Reversed
    }

    public enum StorageType
    {
        Local = 1,
        Firebase
    }

    public enum BenefitType
    {
        Retirement =1,
        Disability,
        Death
    }
    public enum NotificationType
    {
        /// <summary>
        /// Notification sent via email.
        /// </summary>
        Email = 1,

        /// <summary>
        /// Notification sent via SMS (text message).
        /// </summary>
        SMS,

        /// <summary>
        /// Notification sent via push notification on a mobile device.
        /// </summary>
        PushNotification,

        /// <summary>
        /// Notification sent in-app.
        /// </summary>
        InApp,

        /// <summary>
        /// Notification sent through a web-based system or platform.
        /// </summary>
        WebNotification
    }

}
