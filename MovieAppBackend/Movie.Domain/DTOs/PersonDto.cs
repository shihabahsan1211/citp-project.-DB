namespace Movie.Domain.DTOs
{
    public class PersonListItemDto
    {
        public string Nconst { get; set; } = null!;
        public string? PrimaryName { get; set; }
        public string Self { get; set; } = null!;
    }

    public class PersonDetailDto
    {
        public string Nconst { get; set; } = null!;
        public string? PrimaryName { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string? PrimaryProfession { get; set; }
        public string Self { get; set; } = null!;
    }
}
