using Microsoft.AspNetCore.Mvc;
using Movie.Domain.DTOs;
using Movie.Infrastructure.Repositories;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/users/{userId:int}/search-history")]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityRepository _repo;

        public UserActivityController(IUserActivityRepository repo)
        {
            _repo = repo;
        }

        // GET /api/users/11/search-history?limit=20
        [HttpGet]
        public async Task<IActionResult> GetHistory(int userId, [FromQuery] int limit = 20, CancellationToken ct = default)
        {
            var items = await _repo.GetSearchHistoryAsync(userId, limit, ct);

            var result = items.Select(x => new UserSearchHistoryDto
            {
                SearchId = x.SearchId,
                UserId = x.UserId,
                QueryText = x.QueryText,
                PerformedAt = x.PerformedAt
            });

            return Ok(result);
        }

        // POST /api/users/11/search-history
        // Body: { "queryText": "batman" }
        [HttpPost]
        public async Task<IActionResult> AddHistory(int userId, [FromBody] CreateUserSearchHistoryDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.QueryText))
                return BadRequest("queryText is required");

            await _repo.AddSearchAsync(userId, dto.QueryText, ct);
            return NoContent(); // 204 – added successfully
        }
    }
}
