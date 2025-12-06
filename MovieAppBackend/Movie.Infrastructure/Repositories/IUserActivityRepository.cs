using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public interface IUserActivityRepository
    {
        // ---------- SEARCH HISTORY ----------
        Task AddSearchAsync(
            int userId,
            string queryText,
            CancellationToken ct = default);

        Task<IReadOnlyList<UserSearchHistory>> GetSearchHistoryAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default);

        // ---------- TITLE BOOKMARKS ----------
        Task AddTitleBookmarkAsync(
            int userId,
            string tconst,
            CancellationToken ct = default);

        Task<IReadOnlyList<UserBookmarkTitle>> GetTitleBookmarksAsync(
            int userId,
            CancellationToken ct = default);

        // ---------- TITLE RATINGS ----------
        Task<UserTitleRating> AddOrUpdateTitleRatingAsync(
            int userId,
            string tconst,
            int rating,
            CancellationToken ct = default);

        Task<IReadOnlyList<UserTitleRating>> GetTitleRatingsAsync(
            int userId,
            CancellationToken ct = default);
            Task AddPersonBookmarkAsync(int userId, string nconst, CancellationToken ct = default);
            Task<IReadOnlyList<UserBookmarkPerson>> GetPersonBookmarksAsync(int userId, CancellationToken ct = default);

        // ---------- PERSON RATINGS ---------- 
        Task<UserPersonRating> AddOrUpdatePersonRatingAsync(
            int userId,
            string nconst,
            int rating,
            CancellationToken ct = default);

        Task<IReadOnlyList<UserPersonRating>> GetPersonRatingsAsync(
            int userId,
            CancellationToken ct = default);
    
    }
}
