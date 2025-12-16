using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie.Infrastructure.Repositories;
using System.Security.Claims;

namespace Movie.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bookmarks")]
    public class UserBookmarksController : ControllerBase
    {
        private readonly IUserActivityRepository _repo;

        public UserBookmarksController(IUserActivityRepository repo)
        {
            _repo = repo;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Invalid token: user id missing.");
            return userId;
        }

        // POST /api/bookmarks/titles/tt1234567
        [HttpPost("titles/{tconst}")]
        public async Task<IActionResult> AddTitleBookmark(string tconst, CancellationToken ct = default)
        {
            var userId = GetUserId();
            await _repo.AddTitleBookmarkAsync(userId, tconst, ct);
            return NoContent();
        }

        // GET /api/bookmarks/titles
        [HttpGet("titles")]
        public async Task<IActionResult> GetTitleBookmarks(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var items = await _repo.GetTitleBookmarksAsync(userId, ct);

            return Ok(items.Select(x => new
            {
                x.UserId,
                x.Tconst
            }));
        }

        // POST /api/bookmarks/persons/nm0000123
        [HttpPost("persons/{nconst}")]
        public async Task<IActionResult> AddPersonBookmark(string nconst, CancellationToken ct = default)
        {
            var userId = GetUserId();
            await _repo.AddPersonBookmarkAsync(userId, nconst, ct);
            return NoContent();
        }

        // GET /api/bookmarks/persons
        [HttpGet("persons")]
        public async Task<IActionResult> GetPersonBookmarks(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var items = await _repo.GetPersonBookmarksAsync(userId, ct);

            return Ok(items.Select(x => new
            {
                x.UserId,
                x.Nconst
            }));
        }
    }
}
