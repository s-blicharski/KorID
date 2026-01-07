using System.ComponentModel.DataAnnotations;

namespace KorID.API.Models;

public class UpdateUserRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}