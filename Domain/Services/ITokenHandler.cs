using Common.Models;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface ITokenHandler
    {
        Task<Token> CreateAccessTokenAsync(Member member, List<string> roles);
    }
}
