using Persistence.Concrete;

namespace Persistence.DBModels
{
    public class ErrorLog : BaseModel
    {
        public ErrorLog()
        {
            IsResolved = false;
        }
        public string ErrorLogId { get; set; }
        public string ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public string Source { get; set; } // The source of the error (e.g., API, Service, Database)
        public string Method { get; set; } // The method or function where the error occurred
        public string? RequestPath { get; set; } // The endpoint or URL that caused the error
        public string? RequestBody { get; set; } // The request payload
        public string? RequestHeaders { get; set; } // Headers for debugging API-related errors
        public string? MemberId { get; set; } // If the error is related to a specific member
        public string? MemberAgent { get; set; } // The client making the request (e.g., browser, mobile)
        public string? ClientIp { get; set; } // IP address of the request origin
        public bool IsResolved { get; set; } // Status of the error resolution
        public string? ResolvedBy { get; set; } // Who resolved the issue
        public DateTime? ResolvedAt { get; set; } // When the issue was resolved
    }

}
