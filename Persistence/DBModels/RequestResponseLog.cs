using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class RequestResponseLog
    {
        public RequestResponseLog()
        {
            RequestResponseLogId = Ulid.NewUlid().ToString();
            Timestamp = DateTime.Now;
        }
        public string RequestResponseLogId { get; set; }
        public string HttpMethod { get; set; } // GET, POST, PUT, DELETE
        public string RequestPath { get; set; } // API endpoint path
        public string QueryString { get; set; } // Query parameters if any
        public string RequestBody { get; set; } // Request payload
        public string ResponseBody { get; set; } // Response payload
        public int StatusCode { get; set; } // HTTP Status Code (200, 400, 500)
        public string RequestHeaders { get; set; } // Serialized headers
        public string ResponseHeaders { get; set; } // Serialized response headers
        public string MemberId { get; set; } // Member making the request (if authenticated)
        public string ClientIp { get; set; } // IP address of the requester
        public string MemberAgent { get; set; } // Browser, mobile, API client info
        public TimeSpan ExecutionTime { get; set; } // Time taken to process request
        public DateTime RequestInitiatedAt { get; set; } // When request was received
        public DateTime ResponseReceivedAt { get; set; } // When response was sent
        public DateTime Timestamp { get; set; }// Log timestamp
    }

}
