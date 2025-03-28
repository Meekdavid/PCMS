using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    public interface ISMSService
    {
        Task SendAsync(string phoneNumber, string message);
    }
}
