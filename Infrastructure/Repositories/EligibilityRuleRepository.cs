﻿using Application.Interfaces.Database;
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

    public class EligibilityRuleRepository : GenericRepository<EligibilityRule>, IEligibilityRuleRepository
    {
        public EligibilityRuleRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
