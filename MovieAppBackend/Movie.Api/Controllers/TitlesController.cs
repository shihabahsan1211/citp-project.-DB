using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;
using Movie.Domain.DTOs; // 👈 DTOs for clean outer contract

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/titles")]
    public class TitlesController : ControllerBase
    {
        private readonly ImdbDbContext _db;
        public TitlesController(ImdbDbContext db) => _db = db;

        // GET /api/titles?page=1&pageSize=20&q=batman
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Titles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => EF.Functions.ILike(t.PrimaryTitle!, $"%{q}%"));

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.PrimaryTitle)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            // map to DTOs (outer-facing contract)
            var data = items.Select(t => new TitleListItemDto
            {
                Tconst = t.Tconst,
                PrimaryTitle = t.PrimaryTitle,
                Self = $"/api/titles/{t.Tconst}"
            });

            var links = new
            {
                self = Url.Action(null, new { page, pageSize, q }),
                next = page < totalPages ? Url.Action(null, new { page = page + 1, pageSize, q }) : null,
                prev = page > 1 ? Url.Action(null, new { page = page - 1, pageSize, q }) : null
            };

            return Ok(new { page, pageSize, total, totalPages, links, data });
        }

        // GET /api/titles/{tconst}
        [HttpGet("{tconst}")]
        public async Task<IActionResult> GetById(string tconst)
        {
            var t = await _db.Titles.AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Tconst == tconst);

            if (t is null) return NotFound();

            // detail DTO (clear, explicit fields)
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
