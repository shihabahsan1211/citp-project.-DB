namespace Movie.Infrastructure.Models
{
    public class Person
    {
        public string Nconst { get; set; } = null!;
        public string? PrimaryName { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string? PrimaryProfession { get; set; }
    }
}
