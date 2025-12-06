namespace Movie.Domain.DTOs
{
    public class TitleRatingDto
    {
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
