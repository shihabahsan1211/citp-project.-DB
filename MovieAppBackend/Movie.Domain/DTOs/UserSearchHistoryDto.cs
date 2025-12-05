namespace Movie.Domain.DTOs
{
    public class UserSearchHistoryDto
    {
        public int SearchId { get; set; }
        public int UserId { get; set; }
        public string QueryText { get; set; } = null!;
        public DateTime PerformedAt { get; set; }
    }
}
