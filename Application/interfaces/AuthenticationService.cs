using System.Text;
using API.Controllers;
using Domain.Dtos;
using Infrastructure.Interfaces;
using NSec.Cryptography;


namespace Application.Interfaces;

public class AuthenticationService : IAuthenticationService
{
    private readonly List<StoredMessage> _storedMessages = new();
    private readonly Timer _timer;
    public AuthenticationService()
    {
        _timer = new Timer(OnTokensExpire, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private void OnTokensExpire(object? state)
    {
        _storedMessages.RemoveAll(x => x.ExpiresAt < DateTime.UtcNow);
    }

    public string GetMessage(string host, AuthenticationHandshake request)
    {
        var uniqueMessage = Guid.NewGuid().ToString();
        var storedMessage = new StoredMessage
        {
            Name = request.Name,
            Message = uniqueMessage,
            Ip = host,
            Id = request.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _storedMessages.Add(storedMessage);
        return uniqueMessage;
    }

    public bool VerifyMessage(AuthRequest request, string host)
    {
        var bytes = Convert.FromBase64String(request.PublicKey);
        var signatureBytes = Convert.FromBase64String(request.Signature);
        var algorithm = new Ed25519();
        var publicKey = NSec.Cryptography.PublicKey.Import(algorithm, bytes, KeyBlobFormat.RawPublicKey);
        var last = _storedMessages.LastOrDefault(x => x.Ip == host);
        if (last == null) return false;
        
        var messageBytes = Encoding.UTF8.GetBytes(last.Message);
        var isSignatureValid = algorithm.Verify(publicKey, messageBytes, signatureBytes);
        _storedMessages.FirstOrDefault(x => x == last)!.Verified = true;
        _storedMessages.FirstOrDefault(x => x == last)!.PublicKey = request.PublicKey;
        _storedMessages.FirstOrDefault(x => x == last)!.DeviceUuid = request.Uuid;
        
        return isSignatureValid;
    }

    public int ApproveAccount(Guid account, Guid requestOwner)
    {
        
        var entity = _storedMessages.FirstOrDefault(x => x.Id == account);
        if (entity == null)  return -1;
       
        return entity.Verified  ? 2 : 1;
    }
    
    public StoredMessage? GetActiveAuthorizationData(string code)
    {
        var exists = _storedMessages.FirstOrDefault(x => x.Message == code);
        return exists;
    }

    public List<StoredMessage> GetPending()
    {
        return _storedMessages.Where(x => !x.Verified).ToList();
    }
}
