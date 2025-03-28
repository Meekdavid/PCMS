using Application.Interfaces.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SMSService : ISMSService
    {
        public Task SendAsync(string phoneNumber, string message)
        {
            // Simulation of SMS Alert
            return Task.CompletedTask;
        }
    }
}
