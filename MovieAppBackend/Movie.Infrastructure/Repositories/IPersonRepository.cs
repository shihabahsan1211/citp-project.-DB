using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public interface IPersonRepository
    {
        Task<(IReadOnlyList<Person> Items, int Total)> SearchAsync(string? q, int page, int pageSize, CancellationToken ct = default);
        Task<Person?> GetByIdAsync(string nconst, CancellationToken ct = default);
    }
}
