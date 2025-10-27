using Microsoft.AspNetCore.Mvc;
using Movie.Domain.DTOs;
using Movie.Infrastructure.Repositories;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/titles")]
    public class TitlesController : ControllerBase
    {
        private readonly ITitleRepository _repo;
        public TitlesController(ITitleRepository repo) => _repo = repo;

        // GET /api/titles?page=1&pageSize=20&q=batman
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, CancellationToken ct = default)
        {
            var (items, total) = await _repo.SearchAsync(q, page, pageSize, ct);

            var totalPages = (int)Math.Ceiling((double)total / Math.Clamp(pageSize, 1, 100));

            var links = new
            {
                self = Url.Action(null, new { page, pageSize, q }),
                next = page < totalPages ? Url.Action(null, new { page = page + 1, pageSize, q }) : null,
                prev = page > 1 ? Url.Action(null, new { page = page - 1, pageSize, q }) : null
            };

            var data = items.Select(t => new TitleListItemDto
            {
                Tconst = t.Tconst,
                PrimaryTitle = t.PrimaryTitle,
                Self = $"/api/titles/{t.Tconst}"
            });

            return Ok(new { page, pageSize, total, totalPages, links, data });
        }

        // GET /api/titles/tt1234567
        [HttpGet("{tconst}")]
        public async Task<IActionResult> GetById(string tconst, CancellationToken ct = default)
        {
            var t = await _repo.GetByIdAsync(tconst, ct);
            if (t is null) return NotFound();

            var dto = new TitleDetailDto
            {
                Tconst = t.Tconst,
                PrimaryTitle = t.PrimaryTitle,
                TitleType = t.TitleType,
                IsAdult = t.IsAdult,
                StartYear = t.StartYear,
                EndYear = t.EndYear,
                RuntimeMinutes = t.RuntimeMinutes,
                Self = $"/api/titles/{t.Tconst}"
            };

            return Ok(dto);
        }
    }
}
