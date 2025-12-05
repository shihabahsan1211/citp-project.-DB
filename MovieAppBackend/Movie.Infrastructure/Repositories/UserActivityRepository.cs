using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly ImdbDbContext _db;

        public UserActivityRepository(ImdbDbContext db)
        {
            _db = db;
        }

        // ---------- SEARCH HISTORY ----------
        public async Task AddSearchAsync(
            int userId,
            string queryText,
            CancellationToken ct = default)
        {
            var entity = new UserSearchHistory
            {
                UserId = userId,
                QueryText = queryText,
                SearchedAt = DateTime.UtcNow
            };

            _db.UserSearchHistories.Add(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<UserSearchHistory>> GetSearchHistoryAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default)
        {
            return await _db.UserSearchHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SearchedAt)
                .Take(limit)
                .ToListAsync(ct);
        }

        // ---------- TITLE BOOKMARKS ----------
        public async Task AddTitleBookmarkAsync(
            int userId,
            string tconst,
            CancellationToken ct = default)
        {
            var entity = new UserBookmarkTitle
            {
                UserId = userId,
                Tconst = tconst,
                BookmarkedAt = DateTime.UtcNow
            };

            _db.UserBookmarkTitles.Add(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<UserBookmarkTitle>> GetTitleBookmarksAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _db.UserBookmarkTitles
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.BookmarkedAt)
                .ToListAsync(ct);
        }

        // ---------- TITLE RATINGS ----------
        public async Task<UserTitleRating> AddOrUpdateTitleRatingAsync(
            int userId,
            string tconst,
            int rating,
            CancellationToken ct = default)
        {
            var existing = await _db.UserTitleRatings
                .SingleOrDefaultAsync(x => x.UserId == userId && x.Tconst == tconst, ct);

            if (existing is null)
            {
                existing = new UserTitleRating
                {
                    UserId = userId,
                    Tconst = tconst,
                    Rating = rating,
                    RatedAt = DateTime.UtcNow
                };
                _db.UserTitleRatings.Add(existing);
            }
            else
            {
                existing.Rating = rating;
                existing.RatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<IReadOnlyList<UserTitleRating>> GetTitleRatingsAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _db.UserTitleRatings
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.RatedAt)
                .ToListAsync(ct);
        }
    }
}
