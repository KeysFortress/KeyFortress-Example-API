namespace Domain.Dtos;

public class StoredMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public string Ip { get; set; }
    public string DeviceUuid { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Verified { get; set; } = false;
    public string PublicKey { get; set; }
}