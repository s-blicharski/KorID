namespace KorID.Data.Model;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Already-added: store only hash, never plaintext
    public string PasswordHash { get; set; } = null!;

    // Tenant (optional to avoid breaking existing rows)
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Navigation for applications access
    public ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
