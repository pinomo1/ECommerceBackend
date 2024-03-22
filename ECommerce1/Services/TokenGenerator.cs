using ECommerce1.Models;
using ECommerce1.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace ECommerce1.Services
{
    public class TokenGenerator(IOptions<TokenGeneratorOptions> options)
    {
        public TokenGeneratorOptions Options { get; } = options.Value;

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateAccessToken(AuthUser user, string role)
        {
            JwtSecurityTokenHandler handler = new();
            byte[] key = Encoding.ASCII.GetBytes(Options.Secret);
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.Add(Options.AccessExpiration),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = handler.CreateToken(descriptor);
            string accessToken = handler.WriteToken(token);
            return accessToken;
        }
    }
}
