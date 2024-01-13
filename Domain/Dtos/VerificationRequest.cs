using Domain.Enums;

namespace Domain.Dtos;

public class VerificationRequest
{
    public Guid Account { get; set; }
    public required string Code { get; set; }
    public MfaMethodType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
}
