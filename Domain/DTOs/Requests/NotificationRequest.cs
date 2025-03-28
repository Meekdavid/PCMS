using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Requests
{
    public class NotificationRequest
    {
        public NotificationType NotificationType { get; set; }
        public string MemberId { get; set; }
        public string NotificationReference { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
