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

        // GET /api/users/5/search-history?limit=20
        [HttpGet]
        public async Task<IActionResult> GetHistory(
            int userId,
            [FromQuery] int limit = 20,
            CancellationToken ct = default)
        {
            var items = await _repo.GetSearchHistoryAsync(userId, limit, ct);

            var result = items.Select(x => new UserSearchHistoryDto
            {
                Id = x.SearchId,
                QueryText = x.QueryText,
                SearchedOn = x.SearchedAt,
                Self = Url.ActionLink(nameof(GetHistory), values: new { userId }) ?? string.Empty
            });

            return Ok(result);
        }

        // POST /api/users/5/search-history
        [HttpPost]
        public async Task<IActionResult> AddHistory(
            int userId,
            [FromBody] CreateUserSearchHistoryDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.QueryText))
                return BadRequest("QueryText is required");

            var time = dto.SearchedOn ?? DateTime.UtcNow;

            await _repo.AddSearchAsync(userId, dto.QueryText, time, ct);

            return NoContent(); 
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOne(
            int userId,
            int id,
            CancellationToken ct = default)
        {
            var deleted = await _repo.DeleteSearchAsync(userId, id, ct);
            if (!deleted) return NotFound();

            return NoContent(); 
        }

        [HttpDelete]
        public async Task<IActionResult> ClearAll(
            int userId,
            CancellationToken ct = default)
        {
            var deletedCount = await _repo.ClearSearchHistoryAsync(userId, ct);
            return Ok(new { deleted = deletedCount });
        }
    }
}
