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
                // mapped to column search_time in the database
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
                Tconst = tconst
                // NOTE: There is no BookmarkedAt column in the database,
                // so we do not set any timestamp here.
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
                // There is no date column, so we simply order by tconst (or remove this line if not needed).
                .OrderBy(x => x.Tconst)
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
        public async Task AddPersonBookmarkAsync(int userId, string nconst, CancellationToken ct = default)
{
    var entity = new UserBookmarkPerson
    {
        UserId = userId,
        Nconst = nconst
    };

    _db.UserBookmarkPersons.Add(entity);
    await _db.SaveChangesAsync(ct);
}

public async Task<IReadOnlyList<UserBookmarkPerson>> GetPersonBookmarksAsync(
    int userId,
    CancellationToken ct = default)
{
    return await _db.UserBookmarkPersons
        .Where(x => x.UserId == userId)
        .OrderBy(x => x.Nconst)
        .ToListAsync(ct);
}
// ---------- PERSON RATINGS ----------
public async Task<UserPersonRating> AddOrUpdatePersonRatingAsync(
    int userId,
    string nconst,
    int rating,
    CancellationToken ct = default)
{
    var existing = await _db.UserPersonRatings
        .SingleOrDefaultAsync(x => x.UserId == userId && x.Nconst == nconst, ct);

    if (existing is null)
    {
        existing = new UserPersonRating
        {
            UserId = userId,
            Nconst = nconst,
            Rating = rating,
            RatedAt = DateTime.UtcNow
        };
        _db.UserPersonRatings.Add(existing);
    }
    else
    {
        existing.Rating = rating;
        existing.RatedAt = DateTime.UtcNow;
    }

    await _db.SaveChangesAsync(ct);
    return existing;
}

public async Task<IReadOnlyList<UserPersonRating>> GetPersonRatingsAsync(
    int userId,
    CancellationToken ct = default)
{
    return await _db.UserPersonRatings
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.RatedAt)
        .ToListAsync(ct);
}

    }
}
