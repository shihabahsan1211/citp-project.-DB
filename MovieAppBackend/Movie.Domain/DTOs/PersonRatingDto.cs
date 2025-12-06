namespace Movie.Domain.DTOs
{
    public class PersonRatingDto
    {
        public string Nconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
