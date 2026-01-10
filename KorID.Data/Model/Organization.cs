namespace KorID.Data.Model;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<User> Users { get; set; } = new List<User>();
}