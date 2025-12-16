namespace Movie.Domain.DTOs
{
    public class TitleListItemDto
    {
        public string Tconst { get; set; } = null!;
        public string? PrimaryTitle { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }

        public string Self { get; set; } = null!;
    }

    public class TitleDetailDto
    {
        public string Tconst { get; set; } = null!;
        public string? PrimaryTitle { get; set; }
        public string? TitleType { get; set; }
        public bool? IsAdult { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public string Self { get; set; } = null!;
    }
}
