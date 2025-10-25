using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/titles")]
    public class TitlesController : ControllerBase
    {
        private readonly ImdbDbContext _db;
        public TitlesController(ImdbDbContext db)
        {
            _db = db;
        }

        // GET /api/titles?page=1&pageSize=20&q=batman
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
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

            var links = new
            {
                self = Url.Action(null, new { page, pageSize, q }),
                next = page < totalPages ? Url.Action(null, new { page = page + 1, pageSize, q }) : null,
                prev = page > 1 ? Url.Action(null, new { page = page - 1, pageSize, q }) : null
            };

            var data = items.Select(t => new
            {
                tconst = t.Tconst,
                primaryTitle = t.PrimaryTitle,
                self = $"/api/titles/{t.Tconst}"
            });

            return Ok(new { page, pageSize, total, totalPages, links, data });
        }

        // GET /api/titles/tt1234567
        [HttpGet("{tconst}")]
        public async Task<IActionResult> GetById(string tconst)
        {
            var t = await _db.Titles.AsNoTracking().FirstOrDefaultAsync(x => x.Tconst == tconst);
            return t == null
                ? NotFound()
                : Ok(new { t.Tconst, t.PrimaryTitle, self = $"/api/titles/{t.Tconst}" });
        }
    }
}
