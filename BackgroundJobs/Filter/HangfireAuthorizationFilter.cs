using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundJobs.Filter
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public HangfireAuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Authorize(DashboardContext context)
        {
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
            if (environment == "Development" || environment == "Test")
            {
                return true;
            }

            var httpContext = context.GetHttpContext();
            return httpContext.User.IsInRole("Admin");
        }
    }
}
