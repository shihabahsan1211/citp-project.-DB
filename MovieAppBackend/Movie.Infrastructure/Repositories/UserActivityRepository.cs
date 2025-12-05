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

        public async Task AddSearchAsync(int userId, string queryText, CancellationToken ct = default)
        {
            var entity = new UserSearchHistory
            {
                UserId = userId,
                QueryText = queryText,
                PerformedAt = DateTime.UtcNow
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
                .OrderByDescending(x => x.PerformedAt)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
