using System.Text;
using Domain.Dtos;
using Domain.Enums;
using Infrastructure.Interfaces;

namespace Application.Interfaces;

public class VerificationService : IVerificationService
{
    private record EmailInvite
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public Guid SubscriptionId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private List<EmailInvite> _emailInvites { get; set; } = new List<EmailInvite>();
    private List<VerificationRequest> _storedRequests { get; set; } = new List<VerificationRequest>();
    private readonly Timer _timer;
    
    public VerificationService()
    {
        _timer = new Timer(OnTokensExpire, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private void OnTokensExpire(object? state)
    {
        _storedRequests.RemoveAll(x => x.ExpiresAt < DateTime.UtcNow);
        _emailInvites.RemoveAll(x => x.ExpiresAt < DateTime.UtcNow);

    }
    public string CreateRequest(Guid account, MfaMethodType type)
    {
        var code = GenerateRandomCode();
        Console.WriteLine(code);
        _storedRequests.Add(
            new VerificationRequest
            {
                Account = account,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Code = code,
                Type = type
            }
        );
        
        return code;
    }

    
    public string CreateInvite(string accountEmail, Guid subscription)
    {
        var code = GenerateRandomCode();
        Console.WriteLine(code);
        _emailInvites.Add(new EmailInvite
        {
            Code = code,
            SubscriptionId = subscription,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Email = accountEmail
        });
        
        return code;
    }

    public bool VerifyRequest(Guid account, string code, MfaMethodType type)
    {
        var activeRequest = _storedRequests.Where(x => x.Account == account && x.Type == type).ToList();
        if (activeRequest.Count == 0) return false;
        
        return activeRequest.Any(x=> x.Code == code);
    }

    public Guid VerifyEmailInvite(string current, string code)
    {
        var exits = _emailInvites.FirstOrDefault(x => x.Email == current && x.Code == code);
        if (exits != null)
            _emailInvites.Remove(exits);
        
        return exits?.SubscriptionId ?? Guid.Empty;
    }

    private static string GenerateRandomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        
        var code = new StringBuilder(8);
        for (var i = 0; i < 8; i++)
        {
            code.Append(chars[random.Next(chars.Length)]);
        }

        return code.ToString();
    }
}
