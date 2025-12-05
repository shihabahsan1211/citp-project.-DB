public class UserSearchHistory
{
    public int SearchId { get; set; }
    public int UserId { get; set; }
    public string QueryText { get; set; } = null!;
    public DateTime PerformedAt { get; set; }
}
