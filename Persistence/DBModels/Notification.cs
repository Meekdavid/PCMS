using Persistence.Concrete;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class Notification : BaseModel
    {
        public Notification()
        {
            IsSuccess = false;
            Status = Status.Active;
        }
        public string NotificationId { get; set; }
        public string MemberId { get; set; }
        public Member Member { get; set; }
        public string Message { get; set; }
        public string NotificationReference { get; set; }
        public string Subject { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsSuccess { get; set; }
    }

}
