namespace Movie.Domain.DTOs
{
    public class RatePersonRequestDto
    {
        public string Nconst { get; set; } = null!;
        public int Rating { get; set; }  // e.g. 1–10
    }
}
