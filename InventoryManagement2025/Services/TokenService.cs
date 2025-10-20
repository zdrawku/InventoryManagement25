using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManagement2025.Models;
using InventoryManagement2025.Options;
using InventoryManagement2025.Services.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement2025.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly byte[] _signingKey;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new InvalidOperationException("JWT signing key is not configured.");
            }

            _signingKey = Encoding.UTF8.GetBytes(_options.Key);
        }

        public TokenResult CreateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(_signingKey);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Department))
            {
                claims.Add(new Claim("department", user.Department));
            }

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials);

            var handler = new JwtSecurityTokenHandler();
            var serialized = handler.WriteToken(token);

            return new TokenResult(serialized, expires);
        }
    }
}
