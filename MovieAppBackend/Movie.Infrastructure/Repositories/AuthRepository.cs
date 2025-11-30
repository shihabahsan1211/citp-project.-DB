using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Infrastructure.Models;

namespace Movie.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ImdbDbContext _db;
        private readonly IPasswordHasher<AppUser> _hasher;

        public AuthRepository(ImdbDbContext db, IPasswordHasher<AppUser> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
            _db.AppUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Username == username, ct);

        public async Task<AppUser> CreateUserAsync(AppUser user, string password, CancellationToken ct = default)
        {
            user.PasswordHash = _hasher.HashPassword(user, password);
            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }

        public async Task<AppUser?> ValidateUserAsync(string username, string password, CancellationToken ct = default)
        {
            var user = await _db.AppUsers.SingleOrDefaultAsync(u => u.Username == username, ct);
            if (user is null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}
