using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBModels
{
    public class Role : IdentityRole<string>
    {
        public Role(string roleName) : base(roleName)
        {
            this.Id = Ulid.NewUlid().ToString();
        }

        public Role()
        {
            this.Id = Ulid.NewUlid().ToString();
        }
    }
}
