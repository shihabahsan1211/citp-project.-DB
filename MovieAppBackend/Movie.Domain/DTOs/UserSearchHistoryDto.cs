namespace Movie.Domain.DTOs
{
    public class UserSearchHistoryDto
    {
        public int Id { get; set; }        
        public string QueryText { get; set; } = null!;
        public DateTime SearchedOn { get; set; }
        public string? Self { get; set; }
    }
}
