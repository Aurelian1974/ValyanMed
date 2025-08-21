using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using API.Models;

namespace API.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(UtilizatorDb user, string fullName);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UtilizatorDb user, string fullName)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("JWT Key is not configured (Jwt:Key)");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.NumeUtilizator ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("fullName", fullName ?? user.NumeUtilizator ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
