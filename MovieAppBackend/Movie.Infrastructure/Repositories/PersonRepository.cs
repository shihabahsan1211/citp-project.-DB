using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly ImdbDbContext _db;

        public PersonRepository(ImdbDbContext db) => _db = db;

        public async Task<(IReadOnlyList<Person> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Persons.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => EF.Functions.ILike(p.PrimaryName!, $"%{q}%"));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.PrimaryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<Person?> GetByIdAsync(string nconst, CancellationToken ct = default)
            => _db.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.Nconst == nconst, ct);
    }
}
