using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Web.Auth;

public class JwtService(IConfiguration configuration)
{
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
        SecurityAlgorithms.HmacSha256);

    public string Issue(Guid userId) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object> { ["sub"] = userId.ToString() },
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = _credentials
        });
}
