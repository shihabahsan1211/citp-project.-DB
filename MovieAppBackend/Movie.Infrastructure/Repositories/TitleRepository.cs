using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public class TitleRepository : ITitleRepository
    {
        private readonly ImdbDbContext _db;

        public TitleRepository(ImdbDbContext db) => _db = db;

        public async Task<(IReadOnlyList<Title> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Titles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => EF.Functions.ILike(t.PrimaryTitle!, $"%{q}%"));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(t => t.PrimaryTitle)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<Title?> GetByIdAsync(string tconst, CancellationToken ct = default)
            => _db.Titles.AsNoTracking().FirstOrDefaultAsync(x => x.Tconst == tconst, ct);
    }
}
