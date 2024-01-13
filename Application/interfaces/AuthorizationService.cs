using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Application.Interfaces;

public class AuthorizationService : IAuthorizationService
{
    private readonly string _jwtKey;
    private readonly string _issuer;

    public AuthorizationService(IConfiguration configuration)
    {
        _jwtKey = configuration["jwt-key"] ?? string.Empty;
        _issuer = configuration["jwt-issuer"] ?? string.Empty;
    }
    
   
    public string IssueToken(Guid id)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, ""),
            new(ClaimTypes.Role, "Child")
        };

        var bytes = Convert.FromBase64String(_jwtKey);
        var key = new SymmetricSecurityKey(bytes);

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddYears(1);
        var token = new JwtSecurityToken(
            expires: expires,
            signingCredentials: credentials,
            claims: claims,
            issuer: _issuer,
            audience: _issuer,
            notBefore: null
        );


        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    
}
