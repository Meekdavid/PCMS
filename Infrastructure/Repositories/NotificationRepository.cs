using Application.Interfaces.Database;
using Infrastructure.DataAccess.Repositories;
using Persistence.DBContext;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
