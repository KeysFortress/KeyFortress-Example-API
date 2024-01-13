using API.Controllers;
using Domain.Dtos;

namespace Infrastructure.Interfaces;

public interface IAuthenticationService
{
    string GetMessage(string host, AuthenticationHandshake request);
    bool VerifyMessage(AuthRequest request, string host);
    int ApproveAccount(Guid account, Guid requestOwner);
    StoredMessage? GetActiveAuthorizationData(string code);
    List<StoredMessage> GetPending();
}
