using Microsoft.AspNetCore.Mvc;
using Movie.Infrastructure.Repositories;
using Movie.Domain.DTOs;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/users/{userId:int}/bookmarks")]
    public class UserBookmarksController : ControllerBase
    {
        private readonly IUserActivityRepository _repo;

        public UserBookmarksController(IUserActivityRepository repo)
        {
            _repo = repo;
        }

        // =========================================
        // BOOKMARK TITLE
        // =========================================

        // POST /api/users/11/bookmarks/titles/tt1234567
        [HttpPost("titles/{tconst}")]
        public async Task<IActionResult> AddTitleBookmark(
            int userId,
            string tconst,
            CancellationToken ct = default)
        {
            await _repo.AddTitleBookmarkAsync(userId, tconst, ct);
            return NoContent();
        }

        // GET /api/users/11/bookmarks/titles
        [HttpGet("titles")]
        public async Task<IActionResult> GetTitleBookmarks(
            int userId,
            CancellationToken ct = default)
        {
            var items = await _repo.GetTitleBookmarksAsync(userId, ct);

            return Ok(items.Select(x => new
            {
                x.UserId,
                x.Tconst
            }));
        }

        // =========================================
        // BOOKMARK PERSON (optional)
        // =========================================

        // POST /api/users/11/bookmarks/persons/nm0000123
        [HttpPost("persons/{nconst}")]
        public async Task<IActionResult> AddPersonBookmark(
            int userId,
            string nconst,
            CancellationToken ct = default)
        {
            await _repo.AddPersonBookmarkAsync(userId, nconst, ct);
            return NoContent();
        }

        // GET /api/users/11/bookmarks/persons
        [HttpGet("persons")]
        public async Task<IActionResult> GetPersonBookmarks(
            int userId,
            CancellationToken ct = default)
        {
            var items = await _repo.GetPersonBookmarksAsync(userId, ct);

            return Ok(items.Select(x => new
            {
                x.UserId,
                x.Nconst
            }));
        }
    }
}
