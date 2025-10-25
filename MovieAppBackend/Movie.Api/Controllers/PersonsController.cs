using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure.Data;

namespace Movie.Api.Controllers
{
    [ApiController]
    [Route("api/persons")]
    public class PersonsController : ControllerBase
    {
        private readonly ImdbDbContext _db;
        public PersonsController(ImdbDbContext db) => _db = db;

        // GET /api/persons?page=1&pageSize=20&q=tom
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Persons.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => EF.Functions.ILike(p.PrimaryName!, $"%{q}%"));

            var total = await query.CountAsync();
            var items = await query.OrderBy(p => p.PrimaryName)
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

            var data = items.Select(p => new
            {
                nconst = p.Nconst,
                primaryName = p.PrimaryName,
                self = $"/api/persons/{p.Nconst}"
            });

            return Ok(new { page, pageSize, total, totalPages, links, data });
        }

        // GET /api/persons/nm1234567
        [HttpGet("{nconst}")]
        public async Task<IActionResult> GetById(string nconst)
        {
            var person = await _db.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.Nconst == nconst);
            return person == null
                ? NotFound()
                : Ok(new { person.Nconst, person.PrimaryName, self = $"/api/persons/{person.Nconst}" });
        }
    }
}
