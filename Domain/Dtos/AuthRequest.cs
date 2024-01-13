namespace Domain.Dtos;

public class AuthRequest
{
    public required string PublicKey { get; set; }
    public required string Signature { get; set; }
    public required string Uuid { get; set; }
}
