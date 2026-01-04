namespace KorID.API.Models;

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; }
}
