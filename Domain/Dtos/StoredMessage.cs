namespace Domain.Dtos;

public class StoredMessage
{
    public string Message { get; set; }
    public string Ip { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Verified { get; set; } = false;
    public string PublicKey { get; set; }
}