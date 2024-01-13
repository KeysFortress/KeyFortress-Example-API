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
    
    public string IssueHandshakeToken(Guid id)
    {
        
        var claims = new List<Claim>
        {
            new("uuid", id.ToString()),
            new(ClaimTypes.Role, "Mfa")
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);
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

    public string IssueChildToken(Guid id)
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

    public string IssueAuthenticationToken(Guid id, string role)
    {   
        var claims = new List<Claim>
        {
            new("uuid", id.ToString()),
            new(ClaimTypes.Role, role)
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(30);
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
    
    public  ClaimsPrincipal? ValidateAppleIdentityToken(string identityToken)
    {
        try
        {
            var token = ReadToken(identityToken);
            if (token == null) return null;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetApplePublicKey(token.Header.Kid),
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = "com.AirzenKidsWifi.app",
                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(identityToken, tokenValidationParameters, out _);
            
            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }
    
    private JwtSecurityToken? ReadToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (handler.ReadToken(token) is not JwtSecurityToken jsonToken) return null;
        Console.WriteLine("Issuer: " + jsonToken.Issuer);
        Console.WriteLine("Audience: " + jsonToken.Audiences.FirstOrDefault());
        Console.WriteLine("Expiration Time: " + jsonToken.ValidTo);
        Console.WriteLine("Issued At: " + jsonToken.ValidFrom);

        Console.WriteLine("Subject: " + jsonToken.Subject);
        Console.WriteLine("Email: " + jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value);

        Console.WriteLine("\nDecoded Token:\n" + jsonToken);
        return jsonToken;
    }
    
    private static SecurityKey GetApplePublicKey(string keyId)
    {
        var jwksUrl = "https://appleid.apple.com/auth/keys";

        using var httpClient = new HttpClient();
        var json = httpClient.GetStringAsync(jwksUrl).Result;
        var jwt = JsonConvert.DeserializeObject<JsonWebKeySet>(json);
                
        return jwt.Keys.FirstOrDefault(k => k.Kid == keyId);
    }
}
