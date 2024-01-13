using System.Security.Claims;

namespace Infrastructure.Interfaces;

public interface IAuthorizationService
{

    string IssueToken(Guid id);

    

}
