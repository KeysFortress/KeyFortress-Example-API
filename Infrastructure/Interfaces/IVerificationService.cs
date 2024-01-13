using Domain.Enums;

namespace Infrastructure.Interfaces;

public interface IVerificationService
{
    public string CreateRequest(Guid account, MfaMethodType type);
    public string CreateInvite(string accountEmail, Guid subscriptionId);
    public bool VerifyRequest(Guid account, string code, MfaMethodType type);
    public Guid VerifyEmailInvite(string current,  string code);
}
