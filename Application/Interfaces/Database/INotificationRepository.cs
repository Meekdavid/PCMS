using Domain.Interfaces.Database;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Database
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
    }
}
