namespace Movie.Domain.DTOs
{
    // Request body for rating a title
    public class RateTitleRequestDto
    {
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
    }
}
