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
        var algorithm = new Ed25519();
        var bytes = Convert.FromBase64String(request.PublicKey);

        var publicKey = NSec.Cryptography.PublicKey.Import(algorithm, bytes, KeyBlobFormat.RawPublicKey);
        byte[] byteMessage = Encoding.ASCII.GetBytes(uniqueMessage);
        var response = algorithm.Sign(new Key(algorithm), byteMessage);
        var storedMessage = new StoredMessage
        {
            Message = uniqueMessage,
            Ip = host,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _storedMessages.Add(storedMessage);
        return Convert.ToBase64String(response);
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
        
        return isSignatureValid;
    }
    
}
