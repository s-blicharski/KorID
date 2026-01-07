namespace KorID.Data.Model;

public class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;

    // Optional tenant association
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Navigation for many-to-many users <-> applications
    public ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
}