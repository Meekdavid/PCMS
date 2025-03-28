using Persistence.Abstract;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Concrete
{
    public class BaseModel : IBaseModel
    {
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        public BaseModel()
        {
            // Automatically assign value to entityId for all entities inherting from this 
            var entityName = this.GetType().Name;

            var idPropertyName = $"{entityName}Id";
            var idProperty = this.GetType().GetProperty(idPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (idProperty != null && idProperty.PropertyType == typeof(string))
            {
                idProperty.SetValue(this, Ulid.NewUlid().ToString());
            }

            CreatedDate = DateTime.UtcNow;
            Status = Status.Active;
        }
    }
}
