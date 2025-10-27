using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public interface ITitleRepository
    {
        Task<(IReadOnlyList<Title> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default);
        Task<Title?> GetByIdAsync(string tconst, CancellationToken ct = default);
    }
}
