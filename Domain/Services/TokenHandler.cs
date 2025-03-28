using Common.ConfigurationSettings;
using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Persistence.DBModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class TokenHandler : ITokenHandler
    {
        public IConfiguration _configuration { get; set; }
        public TokenHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<Token> CreateAccessToken(Member member, List<string> roles)
        {
            Token token = new Token();

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigSettings.ApplicationSetting.JwtSecret));

            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            token.Expiration = DateTime.UtcNow.AddMinutes(1000);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, member.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
                new Claim(ClaimTypes.Name, member.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            JwtSecurityToken securityToken = new JwtSecurityToken(
                issuer: ConfigSettings.ApplicationSetting.BaseLocalStorageDomain,
                audience: ConfigSettings.ApplicationSetting.BaseLocalStorageDomain,
                expires: token.Expiration,//Token expiration date
                notBefore: DateTime.UtcNow,//Set how long it takes for the token to be activated after it is produced.
                signingCredentials: signingCredentials,
                claims: claims
            );

            //Create token
            string str = ConfigSettings.ApplicationSetting.JwtSecret;
            token.AccessToken = tokenHandler.WriteToken(securityToken);

            //Create refresh token
            token.RefreshToken = CreateRefreshToken();
            return Task.FromResult(token);
        }

        //Refresh token creator
        private string CreateRefreshToken()
        {
            byte[] number = new byte[32];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                random.GetBytes(number);
            return Convert.ToBase64String(number);

        }
        public async Task<Token> CreateAccessTokenAsync(Member member, List<string> roles)
        {
            Token tokenResponse = new Token();
            tokenResponse.Expiration = DateTime.UtcNow.AddMinutes(ConfigSettings.ApplicationSetting.JwtConfig.ExpiryDate);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, member.Id),
                new Claim(JwtRegisteredClaimNames.Email, member.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, member.UserName),
                new Claim(ClaimTypes.NameIdentifier, member.Id),
                new Claim(ClaimTypes.Email, member.Email)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigSettings.ApplicationSetting.JwtConfig.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: ConfigSettings.ApplicationSetting.JwtConfig.Issuer,
                audience: ConfigSettings.ApplicationSetting.JwtConfig.Audience,
                claims: claims,
                expires: tokenResponse.Expiration,
                signingCredentials: creds
            );

            tokenResponse.AccessToken = tokenHandler.WriteToken(token);

            //Create refresh token
            tokenResponse.RefreshToken = CreateRefreshToken();

            return await Task.FromResult(tokenResponse);

        }

    }
}
