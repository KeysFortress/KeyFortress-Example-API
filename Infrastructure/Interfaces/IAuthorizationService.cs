using System.Security.Claims;

namespace Infrastructure.Interfaces;

public interface IAuthorizationService
{
    string IssueHandshakeToken(Guid id);
    string IssueChildToken(Guid id);
    string IssueAuthenticationToken(Guid id, string role);
    ClaimsPrincipal? ValidateAppleIdentityToken(string identityToken);

}
