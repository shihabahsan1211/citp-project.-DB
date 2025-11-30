using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public interface IAuthRepository
    {
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<AppUser> CreateUserAsync(AppUser user, string password, CancellationToken ct = default);
        Task<AppUser?> ValidateUserAsync(string username, string password, CancellationToken ct = default);
        
    }
}
