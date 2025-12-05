namespace Movie.Domain.DTOs
{
    public class UserTitleRatingDto
    {
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
