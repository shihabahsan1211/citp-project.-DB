namespace Movie.Infrastructure.Models
{
    public class AppUser
    {
        public int Id { get; set; }                 // maps to user_id
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
