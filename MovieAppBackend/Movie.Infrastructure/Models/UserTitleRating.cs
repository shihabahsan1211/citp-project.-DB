// user_title_rating
namespace Movie.Infrastructure.Models
{
    public class UserTitleRating
    {
        public int RatingId { get; set; }   // rating_id
        public int UserId { get; set; }     // user_id
        public string Tconst { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }
}
