namespace Movie.Infrastructure.Models
{
    public class UserTitleRating
    {
        public int RatingId { get; set; }   
        public int UserId { get; set; }    
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
