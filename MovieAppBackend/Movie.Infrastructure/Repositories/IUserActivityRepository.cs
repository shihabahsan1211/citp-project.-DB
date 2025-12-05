using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public interface IUserActivityRepository
    {
        // SEARCH HISTORY
        Task AddSearchAsync(int userId, string queryText, CancellationToken ct = default);
        Task<IReadOnlyList<UserSearchHistory>> GetSearchHistoryAsync(int userId, int limit = 20, CancellationToken ct = default);
    }
}
