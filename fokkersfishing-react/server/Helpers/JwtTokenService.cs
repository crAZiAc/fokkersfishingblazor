using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FokkersFishing.Api.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FokkersFishing.Api.Helpers
{
    /// <summary>
    /// Issues signed JWTs for the SPA. Replaces the original IdentityServer flow.
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public int ExpiryHours => int.TryParse(_config["Jwt:ExpiryHours"], out var h) ? h : 8;

        public string CreateToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Jwt:Key is not configured.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(ExpiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
