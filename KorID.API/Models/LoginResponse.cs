namespace KorID.API.Models;

public class LoginResponse
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string Username { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();

}