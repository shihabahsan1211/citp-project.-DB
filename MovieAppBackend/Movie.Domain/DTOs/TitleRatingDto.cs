namespace Movie.Domain.DTOs
{
    public class TitleRatingDto
    {
        public int RatingId { get; set; }
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
