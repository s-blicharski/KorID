namespace KorID.Data.Model;

public class UserApplication
{
    // Composite primary key configured in DbContext
    public int UserId { get; set; }
    public User? User { get; set; }

    public int ApplicationId { get; set; }
    public Application? Application { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}