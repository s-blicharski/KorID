using System.ComponentModel.DataAnnotations;

namespace KorID.API.Models;

public class LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

//Dodajemy required, aby wyeliminować ostrzeżenia o wartościach null.