using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly ImdbDbContext _context;

        public UserActivityRepository(ImdbDbContext context)
        {
            _context = context;
        }

        //SEARCH HISTORY 
        public async Task AddSearchAsync(
            int userId,
            string queryText,
            DateTime searchedAt,
            CancellationToken ct = default)
        {
            var entity = new UserSearchHistory
            {
                UserId = userId,
                QueryText = queryText,
                SearchedAt = searchedAt
            };

            _context.UserSearchHistories.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<UserSearchHistory>> GetSearchHistoryAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default)
        {
            return await _context.UserSearchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.SearchedAt)
                .Take(limit)
                .ToListAsync(ct);
        }

        // delete one history item
        public async Task<bool> DeleteSearchAsync(
            int userId,
            int searchId,
            CancellationToken ct = default)
        {
            var row = await _context.UserSearchHistories
                .FirstOrDefaultAsync(x => x.UserId == userId && x.SearchId == searchId, ct);

            if (row == null) return false;

            _context.UserSearchHistories.Remove(row);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // clear all history
        public async Task<int> ClearSearchHistoryAsync(
            int userId,
            CancellationToken ct = default)
        {
            var rows = await _context.UserSearchHistories
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);

            if (rows.Count == 0) return 0;

            _context.UserSearchHistories.RemoveRange(rows);
            await _context.SaveChangesAsync(ct);
            return rows.Count;
        }

        // TITLE BOOKMARKS
        public async Task AddTitleBookmarkAsync(
            int userId,
            string tconst,
            CancellationToken ct = default)
        {
            var existing = await _context.UserBookmarkTitles
                .FindAsync(new object[] { userId, tconst }, ct);

            if (existing == null)
            {
                var entity = new UserBookmarkTitle
                {
                    UserId = userId,
                    Tconst = tconst
                };

                _context.UserBookmarkTitles.Add(entity);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<UserBookmarkTitle>> GetTitleBookmarksAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _context.UserBookmarkTitles
                .Where(b => b.UserId == userId)
                .ToListAsync(ct);
        }

        //PERSON BOOKMARKS
        public async Task AddPersonBookmarkAsync(
            int userId,
            string nconst,
            CancellationToken ct = default)
        {
            var existing = await _context.UserBookmarkPersons
                .FindAsync(new object[] { userId, nconst }, ct);

            if (existing == null)
            {
                var entity = new UserBookmarkPerson
                {
                    UserId = userId,
                    Nconst = nconst
                };

                _context.UserBookmarkPersons.Add(entity);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<UserBookmarkPerson>> GetPersonBookmarksAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _context.UserBookmarkPersons
                .Where(b => b.UserId == userId)
                .ToListAsync(ct);
        }

        //TITLE RATINGS
        public async Task<UserTitleRating> AddOrUpdateTitleRatingAsync(
            int userId,
            string tconst,
            int rating,
            CancellationToken ct = default)
        {
            var entity = await _context.UserTitleRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Tconst == tconst, ct);

            if (entity == null)
            {
                entity = new UserTitleRating
                {
                    UserId = userId,
                    Tconst = tconst,
                    Rating = rating,
                    RatedAt = DateTime.UtcNow
                };

                _context.UserTitleRatings.Add(entity);
            }
            else
            {
                entity.Rating = rating;
                entity.RatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<IReadOnlyList<UserTitleRating>> GetTitleRatingsAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _context.UserTitleRatings
                .Where(r => r.UserId == userId)
                .ToListAsync(ct);
        }

        //PERSON RATINGS 
        public async Task<UserPersonRating> AddOrUpdatePersonRatingAsync(
            int userId,
            string nconst,
            int rating,
            CancellationToken ct = default)
        {
            var entity = await _context.UserPersonRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Nconst == nconst, ct);

            if (entity == null)
            {
                entity = new UserPersonRating
                {
                    UserId = userId,
                    Nconst = nconst,
                    Rating = rating,
                    RatedAt = DateTime.UtcNow
                };

                _context.UserPersonRatings.Add(entity);
            }
            else
            {
                entity.Rating = rating;
                entity.RatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<IReadOnlyList<UserPersonRating>> GetPersonRatingsAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _context.UserPersonRatings
                .Where(r => r.UserId == userId)
                .ToListAsync(ct);
        }
    }
}
