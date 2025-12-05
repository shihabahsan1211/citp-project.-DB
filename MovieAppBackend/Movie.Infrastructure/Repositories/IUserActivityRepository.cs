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
    }
}
