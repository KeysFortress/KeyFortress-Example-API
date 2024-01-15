namespace API.Controllers;

public class AuthenticationHandshake
{
    public required string PublicKey { get; set; }
    public string ForUser { get; set; }
}

