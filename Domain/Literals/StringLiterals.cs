using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Literals
{
    public class StringLiterals
    {
        /***** STATUS CODES *****/
        public const string ResponseCode_Success = "00";
        public const string ResponseCode_MemberAccountNotFound = "01";
        public const string ResponseCode_TokenNullValue = "02";
        public const string ResponseCode_BadRequest = "03";
        public const string ResponseCode_Unauthorized = "04";
        public const string ResponseCode_PartialContent = "05";
        public const string ResponseCode_Failure = "06";
        public const string ResponseCode_DatabaseConnectionTimeout = "07";
        public const string ResponseCode_StoredProcedureError = "08";
        public const string ResponseCode_ExceptionError = "09";
        public const string ResponseCode_DatabaseConnectionError = "10";
        public const string ResponseCode_DirectoryNotFound = "11";
        public const string ResponseCode_FilesCountMismatch = "12";
        public const string ResponseCode_FilesNotFound = "13";
        public const string ResponseCode_FailedInputValidation = "14";
        public const string ResponseCode_RoleAssignmentFailed = "15";
        public const string ResponseCode_MemberCreationFailed = "16";
        public const string ResponseCode_MemberEmailNotConfirmed = "17";
        public const string ResponseCode_LoginFailed = "18";
        public const string ResponseCode_MemberNotFound = "19";
        public const string ResponseCode_PasswordResetFailed = "20";
        public const string ResponseCode_WrongPassword = "21";
        public const string ResponseCode_UnableToRemovePassword = "22";
        public const string ResponseCode_FailedToAddNewPassword = "23";
        public const string ResponseCode_MemberEmailAlreadyConfirmed = "24";
        public const string ResponseCode_ConfirtmationLinkExpired = "25";
        public const string ResponseCode_FailedToGenerateConfirmationToken = "26";
        public const string ResponseCode_NoFacilityFound = "27";
        public const string ResponseCode_NoCitiesFound = "28";
        public const string ResponseCode_ArticleNotFound = "29";
        public const string ResponseCode_FAQNotFound = "30";
        public const string ResponseCode_CommentNotFound = "30";
        public const string ResponseCode_EmployerNotFound = "31";
        public const string ResponseCode_EmployerAlreadyExists = "32";
        public const string ResponseCode_TransactionProcessingFailed = "33";
        public const string ResponseCode_ContributionNotFound = "13";
        public const string ResponseCode_PensionAccountNotFound = "20";
        public const string ResponseCode_MemberNotEligibleForBenefits = "30";
        public const string ResponseCode_InsufficientFunds = "31";
        public const string ResponseCode_ContributionNotFoundSpecifiedPeriod = "21";
        public const string ResponseCode_AccountNotFound = "47";

        /***** STATUS MESSAGES *****/
        public const string ResponseMessage_Success = "Request Successful.";
        public const string ResponseMessage_SuccessEmailConfirmation = "Email Confirmation Successful.";
        public const string ResponseMessage_Failure = "Request Failed";
        public const string ResponseMessage_Duplicate = "Failed: Duplicate RequestID";
        public const string ResponseMessage_DirectoryNotFound = "Directory not found.";
        public const string ResponseMessage_FilesNotFound = "Files not found.";
        public const string ResponseMessage_CommentNotFound = "Comment not found.";
        public const string ResponseMessage_RoleAssignmentFailed = "Role assignment failed";
        public const string ResponseMessage_WrongInput = "Wrong Input Supplied.";
        public const string ResponseMessage_MemberCreationFailed = "Member creation failed";
        public const string ResponseMessage_ArticleNotFound = "Article Not Found";
        public const string ResponseMessage_FAQNotFound = "FAQ Not Found";
        public const string ResponseMessage_MemberEmailNotConfirmed = "Please confirm email";
        public const string ResponseMessage_MemberEmailAlreadyConfirmed = "Please confirm email";
        public const string ResponseMessage_NoFacilityFound = "No Facilities Found in {Region}";
        public const string ResponseMessage_FailedToGenerateConfirmationToken = "Failed to Generate Confirmation Token";
        public const string ResponseMessage_ConfirmationMailSent = "Confirmation link sent to your email";
        public const string ResponseMessage_MemberNotFound = "Member not found.";
        public const string ResponseMessage_PasswordResetFailed = "Password reset failed.";
        public const string ResponseMessage_ConfirtmationLinkExpired = "Confirmation token expired.";
        public const string ResponseMessage_FailedToAddNewPassword = "Failed to Add New Password for Member.";
        public const string ResponseMessage_WrongPassword = "Wrong Password.";
        public const string ResponseMessage_UnableToRemovePassword = "Unable to remove password for member.";
        public const string ResponseMessage_LoginFailed = "Login failed";
        public const string ResponseMessage_FilesCountMismatch = "Path and file count mismatch.";
        public const string ResponseMessage_UnknownError = "Unknown Error Occured while Performing this Action.";
        public const string ResponseMessage_TokenNullValue = "Authorization Token Value is Null";
        public const string ResponseMessage_BadRequest = "Required request parameter is Invalid / Missing";
        public const string ResponseMessage_Unauthorized = "Authentication Token is Unauthorized";
        public const string ResponseMessage_DatabaseConnectionTimeout = "Database Connection Timeout";
        public const string ResponseMessage_StoredProcedureError = "Stored Procedured Execution Failed";
        public const string ResponseMessage_ExceptionError = "An Exception Occured";
        public const string ResponseMessage_DatabaseConnectionError = "Database Connection Error";
        public const string ResponseMessage_AccountNameNotFound = "Merchant Details Not Found";
        public const string ResponseMessage_TransactionNotFound = "Transaction Information Not Found";
        public const string ResponseMessage_AuthenticationFailue = "Unable to Authenticate Merchant, Please Try Again Later!";
        public const string ResponseMessage_InputFailure = "Header Value 'Merchant ID' Contains Disallowed Special Characters";
        public const string ResponseMessage_DateFailure = "Date Format Supplied does not Match Server Date, Format expected is {serverDateFormat}";
        public const string ResponseMessage_TransactionExceeding = "Maximumn Number of Days to Request Transaction History Per Time Exceeded";
        public const string ResponseMessage_IncorrectCredentials = "Incorrect Login Credentials Provided";
        public const string ResponseMessage_EmployerNotFound = "Employer not found";
        public const string ResponseMessage_EmployerAlreadyExists = "Employer Already Exists";
        public const string ResponseMessage_TransactionProcessingFailed = "Transaction processing failed";
        public const string ResponseMessage_ContributionNotFound = "Contribution not found";
        public const string ResponseMessage_PensionAccountNotFound = "Pension account not found";
        public const string ResponseMessage_InsufficientFunds = "Insufficient funds for withdrawal";
        public const string ResponseMessage_MemberNotEligibleForBenefits = "Member is not eligible for benefits withdrawal";
        public const string ResponseMessage_ContributionNotFoundSpecifiedPeriod = "No contributions found for the specified period";
        public const string ResponseMessage_AccountNotFound = "No payment account found";

    }
}
