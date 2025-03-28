using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ConfigurationSettings.ConfigModels
{
    public class ApplicationSettings
    {
        public EmailDetails EmailDetails { get; set; }
        public FireBaseStorage FireBaseStorage { get; set; }
        public JwtConfig JwtConfig { get; set; }
        public string JwtSecret { get; set; }
        public string MaximumTokenEdebAI { get; set; }
        public string BaseLocalStorageDomain { get; set; }
        public string NLPCPensionHomePage { get; set; }
        public string NLPCPensionUnsubscribeLink { get; set; }
        public string MedicalFacilityEndpoint { get; set; }
        public string CitiesofSpecifiedCountry { get; set; }
        public string CitiesofSpecifiedState { get; set; }
        public string StatesofSpecifiedCountry { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
        public int MinimunRequiredAge { get; set; }
        public int MaximumRequiredAge { get; set; }
        public int MaximumFileSizeUpload { get; set; }
        public int RetryCountForDatabaseTransactions { get; set; }
        public int RetryCountForExceptions { get; set; }
        public int SecondsBetweenEachRetry { get; set; }
        public int CacheDuration { get; set; }
        public int MinimumAgeForEligibility { get; set; }
        public int MinimumBalanceForInterest { get; set; }
        public decimal PensionAnualRate { get; set; }
        public string NLPCBank { get; set; }
        public string NLPCAccountId { get; set; }
        public string NLPCInterestAccountId { get; set; }
    }
}
